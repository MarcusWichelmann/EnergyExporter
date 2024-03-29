using System;
using System.Threading;
using System.Threading.Tasks;
using EnergyExporter.InfluxDb;
using EnergyExporter.Mqtt;
using EnergyExporter.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EnergyExporter.Services;

public class DevicePollingService : IHostedService, IDisposable
{
    private readonly ILogger<DevicePollingService> _logger;
    private readonly DeviceService _deviceService;
    private readonly IOptions<PollingOptions> _pollingOptions;
    private readonly InfluxDbExporter _influxDbExporter;
    private readonly MqttExporter _mqttExporter;

    private Timer? _timer;

    public DevicePollingService(
        ILogger<DevicePollingService> logger,
        DeviceService deviceService,
        IOptions<PollingOptions> pollingOptions,
        InfluxDbExporter influxDbExporter,
        MqttExporter mqttExporter)
    {
        _logger = logger;
        _deviceService = deviceService;
        _pollingOptions = pollingOptions;
        _influxDbExporter = influxDbExporter;
        _mqttExporter = mqttExporter;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting device polling.");

        // Do an initial update
        await PollDevicesAsync();

        // Start the timer
        TimeSpan interval = TimeSpan.FromSeconds(_pollingOptions.Value.IntervalSeconds);
        _timer = new Timer(OnTimerTick, null, interval, interval);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping device polling.");

        // Disable the timer
        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    private async void OnTimerTick(object? state) => await PollDevicesAsync();

    private async Task PollDevicesAsync()
    {
        _logger.LogDebug("Polling devices.");
        try
        {
            await _deviceService.QueryDevicesAsync();
            _logger.LogDebug("Polling completed.");

            _logger.LogDebug("Pushing metrics.");
            await _influxDbExporter.PushMetricsAsync();
            await _mqttExporter.PublishMetricsAsync();
            _logger.LogDebug("Pushing completed.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Polling failed.");
        }
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
