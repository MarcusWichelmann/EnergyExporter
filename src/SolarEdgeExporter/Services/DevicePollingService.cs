using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SolarEdgeExporter.Options;

namespace SolarEdgeExporter.Services
{
    public class DevicePollingService : IHostedService, IDisposable
    {
        private readonly ILogger<DevicePollingService> _logger;
        private readonly DeviceService _deviceService;
        private readonly IOptions<PollingOptions> _pollingOptions;

        private Timer? _timer;

        public DevicePollingService(ILogger<DevicePollingService> logger, DeviceService deviceService,
            IOptions<PollingOptions> pollingOptions)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
            _pollingOptions = pollingOptions ?? throw new ArgumentNullException(nameof(pollingOptions));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting device polling...");

            // Do an initial update
            await PollDevicesAsync();

            // Start the timer
            TimeSpan interval = TimeSpan.FromSeconds(_pollingOptions.Value.IntervalSeconds);
            _timer = new Timer(OnTimerTick, null, interval, interval);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping device polling...");

            // Disable the timer
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        private async void OnTimerTick(object? state) => await PollDevicesAsync();

        private async Task PollDevicesAsync()
        {
            _logger.LogDebug("Polling devices...");
            try
            {
                await _deviceService.QueryDevicesAsync();
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
}
