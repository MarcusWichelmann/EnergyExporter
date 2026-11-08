using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using EnergyExporter.Devices;
using FluentModbus;
using Microsoft.Extensions.Logging;

namespace EnergyExporter.Modbus;

public sealed class ModbusReader : IDisposable
{
    /// <summary>
    /// The modbus protocol limits a single "read holding registers" request to 125 registers.
    /// </summary>
    private const int MaxRegistersPerRequest = 125;

    private readonly ILogger<ModbusReader> _logger;

    private readonly string _host;
    private readonly ushort _port;
    private readonly ModbusReaderSettings _settings;

    private TcpClient? _tcpClient;
    private readonly ModbusTcpClient _modbusClient = new();
    private readonly SemaphoreSlim _modbusLock = new(1);

    private volatile bool _reconnectRequested;

    public string Endpoint => $"{_host}:{_port}";

    public bool IsConnected => _tcpClient?.Connected == true;

    public ModbusReader(ILogger<ModbusReader> logger, string host, ushort port, ModbusReaderSettings settings)
    {
        _logger = logger;
        _host = host;
        _port = port;
        _settings = settings;
    }

    /// <summary>
    /// Drops the current connection, so the next read starts with a fresh one.
    /// </summary>
    public void RequestReconnect() => _reconnectRequested = true;

    public async Task<TDevice> ReadDeviceAsync<TDevice>(
        byte unit,
        ushort startRegister,
        CancellationToken cancellationToken = default) where TDevice : IDevice
    {
        // Build the read plan before touching the network, so mapping errors are never retried.
        IReadOnlyList<(ushort Start, ushort Count)> chunks = GetChunksToRead<TDevice>();
        int registerCount = chunks.Max(c => c.Start + c.Count);

        await _modbusLock.WaitAsync(cancellationToken);
        try
        {
            for (var attempt = 0;; attempt++)
            {
                var reusedConnection = false;
                try
                {
                    if (_reconnectRequested)
                    {
                        _reconnectRequested = false;
                        Disconnect();
                    }

                    if (IsConnected)
                        reusedConnection = true;
                    else
                        await ConnectAsync(cancellationToken);

                    _logger.LogDebug(
                        "Reading {Device} at address {StartRegister} from {Endpoint} and unit {Unit}.",
                        typeof(TDevice).Name,
                        $"0x{startRegister:x}",
                        Endpoint,
                        unit);

                    Memory<byte> data = new byte[registerCount * ModbusUtils.SingleRegisterSize];
                    foreach ((ushort chunkStart, ushort chunkSize) in chunks)
                    {
                        Memory<byte> chunkData = await WithTimeout(
                            ct => _modbusClient.ReadHoldingRegistersAsync(
                                unit,
                                (ushort)(startRegister + chunkStart),
                                chunkSize,
                                ct),
                            _settings.ReadTimeout,
                            $"Reading {chunkSize} registers at 0x{startRegister + chunkStart:x}",
                            cancellationToken);

                        if (chunkData.Length != chunkSize * ModbusUtils.SingleRegisterSize)
                            throw new ModbusReadException(
                                $"Reading registers chunk failed: Expected {chunkSize * ModbusUtils.SingleRegisterSize} bytes but received {chunkData.Length}.");
                        chunkData.CopyTo(data[(chunkStart * ModbusUtils.SingleRegisterSize)..]);
                    }

                    return CreateDeviceInstance<TDevice>(data.Span);
                }
                catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
                {
                    // Whatever went wrong, the connection state is unknown now. Start over with a fresh one.
                    Disconnect();

                    // Retrying only helps when an existing connection went stale (e.g. after the inverter's
                    // network module restarted). If a fresh connection failed, the device is really unreachable
                    // and retrying would just double the time spent waiting.
                    if (!reusedConnection || attempt >= _settings.Retries)
                        throw;

                    _logger.LogDebug(
                        "Reading {Device} from {Endpoint} failed ({Error}), retrying with a new connection.",
                        typeof(TDevice).Name,
                        Endpoint,
                        ex.Message);
                }
            }
        }
        finally
        {
            _modbusLock.Release();
        }
    }

    private async Task ConnectAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Connecting to modbus server at {Endpoint}.", Endpoint);

        Disconnect();

        var tcpClient = new TcpClient();
        try
        {
            await WithTimeout(
                async ct => {
                    await tcpClient.ConnectAsync(_host, _port, ct);
                    return true;
                },
                _settings.ConnectTimeout,
                $"Connecting to {Endpoint}",
                cancellationToken);
        }
        catch
        {
            tcpClient.Dispose();
            throw;
        }

        _tcpClient = tcpClient;
        _modbusClient.ReadTimeout = (int)_settings.ReadTimeout.TotalMilliseconds;
        _modbusClient.WriteTimeout = (int)_settings.ReadTimeout.TotalMilliseconds;
        _modbusClient.Initialize(_tcpClient, ModbusEndianness.LittleEndian);

        _logger.LogDebug("Modbus connection to {Endpoint} established.", Endpoint);
    }

    private void Disconnect()
    {
        TcpClient? tcpClient = _tcpClient;
        _tcpClient = null;
        tcpClient?.Dispose();
    }

    /// <summary>
    /// Runs a network operation with a hard deadline. Async socket operations do not honour
    /// socket-level timeouts, so without this a half-open connection could hang forever.
    /// </summary>
    private static async Task<T> WithTimeout<T>(
        Func<CancellationToken, Task<T>> operation,
        TimeSpan timeout,
        string description,
        CancellationToken cancellationToken)
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(timeout);
        try
        {
            return await operation(timeoutCts.Token).WaitAsync(timeout, cancellationToken);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested
                                   && ex is OperationCanceledException or TimeoutException)
        {
            throw new TimeoutException($"{description} timed out after {timeout.TotalSeconds:0.#}s.");
        }
    }

    /// <summary>
    /// Groups the registers required by a device into as few contiguous requests as possible.
    /// </summary>
    internal static IReadOnlyList<(ushort Start, ushort Count)> GetChunksToRead<TDevice>()
    {
        ushort[] addresses = typeof(TDevice).GetProperties()
            .SelectMany(
                prop => Attribute.GetCustomAttribute(prop, typeof(ModbusRegisterAttribute)) is ModbusRegisterAttribute a
                    ? a.GetRelativeAddressesToRead(prop.PropertyType)
                    : Enumerable.Empty<ushort>())
            .Distinct()
            .OrderBy(r => r)
            .ToArray();

        if (addresses.Length == 0)
            throw new ModbusReadException($"Could not find any register addresses to read for {typeof(TDevice).Name}.");

        var chunks = new List<(ushort, ushort)>();
        ushort start = addresses[0];
        ushort end = start;
        foreach (ushort address in addresses.Skip(1))
        {
            if (address == end + 1 && address - start < MaxRegistersPerRequest)
            {
                end = address;
                continue;
            }

            chunks.Add((start, (ushort)(end - start + 1)));
            start = end = address;
        }

        chunks.Add((start, (ushort)(end - start + 1)));
        return chunks;
    }

    private static TDevice CreateDeviceInstance<TDevice>(ReadOnlySpan<byte> registers) where TDevice : IDevice
    {
        var device = Activator.CreateInstance<TDevice>();

        foreach (PropertyInfo property in typeof(TDevice).GetProperties())
        {
            if (Attribute.GetCustomAttribute(property, typeof(ModbusRegisterAttribute)) is not ModbusRegisterAttribute
                modbusRegisterAttribute)
                continue;

            object value = modbusRegisterAttribute.Read(registers, property.PropertyType);
            property.SetValue(device, value);
        }

        return device;
    }

    public void Dispose()
    {
        Disconnect();
        _modbusClient.Dispose();
        _modbusLock.Dispose();
    }
}

public record ModbusReaderSettings(TimeSpan ConnectTimeout, TimeSpan ReadTimeout, int Retries);
