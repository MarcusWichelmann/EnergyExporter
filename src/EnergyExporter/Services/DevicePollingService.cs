using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnergyExporter.InfluxDb;
using EnergyExporter.Mqtt;
using EnergyExporter.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EnergyExporter.Services;

/// <summary>
/// Polls all devices on a fixed interval. A poll never overlaps with the previous one, and polling
/// starts in the background so the web UI and metrics endpoint are available immediately.
/// </summary>
public class DevicePollingService : BackgroundService
{
    private readonly ILogger<DevicePollingService> _logger;
    private readonly DeviceService _deviceService;
    private readonly IOptions<PollingOptions> _pollingOptions;
    private readonly InfluxDbExporter _influxDbExporter;
    private readonly MqttExporter _mqttExporter;
    private readonly StatusEventLog _eventLog;

    private readonly SemaphoreSlim _pollNowSignal = new(0, 1);
    private bool _overrunReported;
    private readonly ConcurrentDictionary<string, ExporterStatus> _exporterStatuses = new();

    public DateTimeOffset StartedAt { get; } = DateTimeOffset.UtcNow;

    public TimeSpan Interval => TimeSpan.FromSeconds(_pollingOptions.Value.IntervalSeconds);

    public DateTimeOffset? NextPoll { get; private set; }

    public bool IsPolling { get; private set; }

    public DevicePollingService(
        ILogger<DevicePollingService> logger,
        DeviceService deviceService,
        IOptions<PollingOptions> pollingOptions,
        InfluxDbExporter influxDbExporter,
        MqttExporter mqttExporter,
        StatusEventLog eventLog)
    {
        _logger = logger;
        _deviceService = deviceService;
        _pollingOptions = pollingOptions;
        _influxDbExporter = influxDbExporter;
        _mqttExporter = mqttExporter;
        _eventLog = eventLog;

        _exporterStatuses["InfluxDB"] = new ExporterStatus("InfluxDB", influxDbExporter.Enabled, null, null, null, null);
        _exporterStatuses["MQTT"] = new ExporterStatus("MQTT", mqttExporter.Enabled, null, null, null, null);
    }

    public IReadOnlyCollection<ExporterStatus> ExporterStatuses => _exporterStatuses.Values.ToArray();

    /// <summary>
    /// Starts a poll right away instead of waiting for the next interval.
    /// </summary>
    public void PollNow()
    {
        try
        {
            _pollNowSignal.Release();
        }
        catch (SemaphoreFullException)
        {
            // A poll is already requested.
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting device polling every {Interval}s.", Interval.TotalSeconds);
        _eventLog.Add("Info", "Exporter", $"Started, polling every {Interval.TotalSeconds}s.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var stopwatch = Stopwatch.StartNew();
            await PollDevicesAsync(stoppingToken);

            TimeSpan delay = Interval - stopwatch.Elapsed;
            if (delay < TimeSpan.Zero)
            {
                if (!_overrunReported)
                {
                    _logger.LogWarning(
                        "Polling took {Elapsed:0.0}s, which is longer than the {Interval}s polling interval. "
                        + "This usually means a device is not responding.",
                        stopwatch.Elapsed.TotalSeconds,
                        Interval.TotalSeconds);
                    _overrunReported = true;
                }

                delay = TimeSpan.Zero;
            }
            else
            {
                _overrunReported = false;
            }

            NextPoll = DateTimeOffset.UtcNow + delay;
            try
            {
                await _pollNowSignal.WaitAsync(delay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("Stopped device polling.");
    }

    private async Task PollDevicesAsync(CancellationToken cancellationToken)
    {
        IsPolling = true;
        try
        {
            _logger.LogDebug("Polling devices.");
            await _deviceService.QueryDevicesAsync(cancellationToken);

            // Exporters are independent: a broken MQTT broker must not stop InfluxDB pushes and vice versa.
            await RunExporterAsync("InfluxDB", _influxDbExporter.Enabled, _influxDbExporter.PushMetricsAsync);
            await RunExporterAsync("MQTT", _mqttExporter.Enabled, _mqttExporter.PublishMetricsAsync);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Polling failed.");
            _eventLog.Add("Error", "Exporter", $"Polling failed: {ex.Message}");
        }
        finally
        {
            IsPolling = false;
        }
    }

    private async Task RunExporterAsync(string name, bool enabled, Func<Task> export)
    {
        if (!enabled)
            return;

        ExporterStatus previous = _exporterStatuses[name];
        try
        {
            await export();
            if (previous.Healthy == false)
            {
                _logger.LogInformation("{Exporter} export is working again.", name);
                _eventLog.Add("Info", name, "Export working again.");
            }

            _exporterStatuses[name] = previous with { Healthy = true, LastSuccess = DateTimeOffset.UtcNow };
        }
        catch (Exception ex)
        {
            if (previous.Healthy != false)
            {
                _logger.LogWarning(ex, "{Exporter} export failed.", name);
                _eventLog.Add("Warning", name, $"Export failed: {ex.Message}");
            }
            else
            {
                _logger.LogDebug(ex, "{Exporter} export is still failing.", name);
            }

            _exporterStatuses[name] = previous with {
                Healthy = false,
                LastFailure = DateTimeOffset.UtcNow,
                LastError = ex.Message
            };
        }
    }

    public override void Dispose()
    {
        _pollNowSignal.Dispose();
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
