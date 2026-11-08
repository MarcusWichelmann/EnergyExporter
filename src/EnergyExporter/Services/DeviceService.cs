using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using EnergyExporter.Devices;
using EnergyExporter.Modbus;
using EnergyExporter.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EnergyExporter.Services;

public class DeviceService
{
    private readonly ILogger<DeviceService> _logger;
    private readonly ModbusReaderPool _modbusReaderPool;
    private readonly StatusEventLog _eventLog;

    private readonly IReadOnlyList<(DevicesOptions.ModbusDevice Config, DeviceStatus Status)> _configuredDevices;

    private IImmutableList<IDevice> _devices = ImmutableList<IDevice>.Empty;
    private readonly object _lock = new();

    /// <summary>
    /// Devices that were read successfully in the most recent poll. Only these are exported.
    /// </summary>
    public IImmutableList<IDevice> Devices
    {
        get
        {
            lock (_lock)
            {
                return _devices;
            }
        }
    }

    public DateTimeOffset? LastPoll { get; private set; }

    public DeviceService(
        ILogger<DeviceService> logger,
        IOptions<DevicesOptions> devicesOptions,
        ModbusReaderPool modbusReaderPool,
        StatusEventLog eventLog)
    {
        _logger = logger;
        _modbusReaderPool = modbusReaderPool;
        _eventLog = eventLog;

        _configuredDevices = devicesOptions.Value.ModbusDevices
            .Select((config, id) => (config, CreateStatus(id, config)))
            .ToList();
    }

    public IReadOnlyList<DeviceStatus> GetStatuses()
    {
        lock (_lock)
        {
            return _configuredDevices.Select(d => d.Status.Snapshot()).ToList();
        }
    }

    /// <summary>
    /// Drops the modbus connection used by the given device (or all devices), so the next poll reconnects.
    /// </summary>
    public bool RequestReconnect(int? deviceId = null)
    {
        if (deviceId == null)
        {
            _modbusReaderPool.RequestReconnectAll();
            _eventLog.Add("Info", "UI", "Reconnect requested for all devices.");
            return true;
        }

        if (deviceId < 0 || deviceId >= _configuredDevices.Count)
            return false;

        DevicesOptions.ModbusDevice config = _configuredDevices[deviceId.Value].Config;
        _modbusReaderPool.ReaderFor(config.Host, config.Port).RequestReconnect();
        _eventLog.Add("Info", "UI", $"Reconnect requested for {config.Host}:{config.Port}.");
        return true;
    }

    public async Task QueryDevicesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Querying devices.");

        IDevice?[] devices = await Task.WhenAll(
            _configuredDevices.Select(d => QueryAndTrackAsync(d.Config, d.Status, cancellationToken)));

        lock (_lock)
        {
            _devices = devices.Where(d => d != null).Cast<IDevice>().ToImmutableList();
        }

        LastPoll = DateTimeOffset.UtcNow;
    }

    private async Task<IDevice?> QueryAndTrackAsync(
        DevicesOptions.ModbusDevice config,
        DeviceStatus status,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        try
        {
            IDevice device = config switch {
                DevicesOptions.SolarEdgeModbusDevice solarEdge => await QuerySolarEdgeDeviceAsync(
                    solarEdge,
                    cancellationToken),
                DevicesOptions.JanitzaModbusDevice janitza => await QueryJanitzaDeviceAsync(janitza, cancellationToken),
                _ => throw new ArgumentOutOfRangeException(nameof(config), config.GetType().Name, null)
            };

            lock (_lock)
            {
                if (!status.Online && status.TotalPolls > 0)
                {
                    TimeSpan downtime = now - (status.OfflineSince ?? now);
                    _logger.LogInformation(
                        "{Device} is reachable again after {Failures} failed polls ({Downtime:g}).",
                        status.Name,
                        status.ConsecutiveFailures,
                        downtime);
                    _eventLog.Add(
                        "Info",
                        status.Name,
                        $"Back online after {status.ConsecutiveFailures} failed polls ({downtime:hh\\:mm\\:ss}).");
                }
                else if (status.TotalPolls == 0)
                {
                    _logger.LogInformation("{Device} is online.", status.Name);
                    _eventLog.Add("Info", status.Name, "Online.");
                }

                status.Online = true;
                status.OfflineSince = null;
                status.ConsecutiveFailures = 0;
                status.LastSuccess = now;
                status.LastReading = device;
            }

            return device;
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            string message = DescribeError(ex);
            lock (_lock)
            {
                if (status.Online || status.TotalPolls == 0)
                {
                    // Log the first failure of an outage as a warning. Follow-up failures are only logged at
                    // debug level, so an inverter that is offline at night does not flood the logs.
                    _logger.LogWarning("{Device} could not be read: {Error}", status.Name, message);
                    _logger.LogDebug(ex, "Details for failed read of {Device}.", status.Name);
                    _eventLog.Add("Warning", status.Name, $"Offline: {message}");
                    status.OfflineSince = now;
                }
                else
                {
                    _logger.LogDebug(ex, "{Device} is still unreachable.", status.Name);
                    if (status.LastError != message)
                        _eventLog.Add("Warning", status.Name, message);
                }

                status.Online = false;
                status.ConsecutiveFailures++;
                status.TotalFailures++;
                status.LastFailure = now;
                status.LastError = message;
            }

            return null;
        }
        finally
        {
            lock (_lock)
            {
                status.TotalPolls++;
                status.LastAttempt = now;
                status.LastDurationMs = stopwatch.Elapsed.TotalMilliseconds;
            }
        }
    }

    private static string DescribeError(Exception ex) =>
        ex switch {
            SocketException se => $"{se.SocketErrorCode}: {se.Message}",
            _ when ex.InnerException is SocketException ise => $"{ise.SocketErrorCode}: {ise.Message}",
            _ => ex.Message
        };

    private async Task<IDevice> QuerySolarEdgeDeviceAsync(
        DevicesOptions.SolarEdgeModbusDevice config,
        CancellationToken cancellationToken)
    {
        ModbusReader reader = _modbusReaderPool.ReaderFor(config.Host, config.Port);

        SolarEdgeDevice device = config.Type switch {
            DevicesOptions.SolarEdgeModbusDeviceType.Inverter => await reader.ReadDeviceAsync<SolarEdgeInverter>(
                config.Unit,
                SolarEdgeInverter.ModbusAddress,
                cancellationToken),
            DevicesOptions.SolarEdgeModbusDeviceType.Meter => await reader.ReadDeviceAsync<SolarEdgeMeter>(
                config.Unit,
                SolarEdgeMeter.ModbusAddresses[config.Index],
                cancellationToken),
            DevicesOptions.SolarEdgeModbusDeviceType.Battery => await reader.ReadDeviceAsync<SolarEdgeBattery>(
                config.Unit,
                SolarEdgeBattery.ModbusAddresses[config.Index],
                cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(config), config.Type, null)
        };

        if (config.SerialNumberOverride != null)
            device.SerialNumber = config.SerialNumberOverride;

        return device;
    }

    private async Task<IDevice> QueryJanitzaDeviceAsync(
        DevicesOptions.JanitzaModbusDevice config,
        CancellationToken cancellationToken)
    {
        ModbusReader reader = _modbusReaderPool.ReaderFor(config.Host, config.Port);

        return config.Type switch {
            DevicesOptions.JanitzaModbusDeviceType.PowerAnalyzer => await reader.ReadDeviceAsync<JanitzaPowerAnalyzer>(
                config.Unit,
                JanitzaPowerAnalyzer.ModbusAddress,
                cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(config), config.Type, null)
        };
    }

    private static DeviceStatus CreateStatus(int id, DevicesOptions.ModbusDevice config) =>
        config switch {
            DevicesOptions.SolarEdgeModbusDevice se => new DeviceStatus {
                Id = id,
                Name = se.Type == DevicesOptions.SolarEdgeModbusDeviceType.Inverter
                    ? $"SolarEdge Inverter ({se.Host}, unit {se.Unit})"
                    : $"SolarEdge {se.Type} #{se.Index} ({se.Host}, unit {se.Unit})",
                Vendor = "SolarEdge",
                Type = se.Type.ToString(),
                Host = se.Host,
                Port = se.Port,
                Unit = se.Unit,
                Index = se.Type == DevicesOptions.SolarEdgeModbusDeviceType.Inverter ? null : se.Index
            },
            DevicesOptions.JanitzaModbusDevice ja => new DeviceStatus {
                Id = id,
                Name = $"Janitza {ja.Type} ({ja.Host}, unit {ja.Unit})",
                Vendor = "Janitza",
                Type = ja.Type.ToString(),
                Host = ja.Host,
                Port = ja.Port,
                Unit = ja.Unit
            },
            _ => throw new ArgumentOutOfRangeException(nameof(config), config.GetType().Name, null)
        };
}
