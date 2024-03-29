using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using EnergyExporter.Devices;
using EnergyExporter.Options;
using EnergyExporter.Services;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EnergyExporter.InfluxDb;

public class InfluxDbExporter : IDisposable
{
    private readonly ILogger<InfluxDbExporter> _logger;
    private readonly IOptions<ExportOptions> _exportOptions;
    private readonly DeviceService _deviceService;

    private readonly InfluxDBClient? _influxDbClient;

    public InfluxDbExporter(
        ILogger<InfluxDbExporter> logger,
        IOptions<ExportOptions> exportOptions,
        DeviceService deviceService)
    {
        _logger = logger;
        _exportOptions = exportOptions;
        _deviceService = deviceService;

        ExportOptions.InfluxDbOptions? influxDbOptions = exportOptions.Value.InfluxDb;
        if (influxDbOptions != null)
            _influxDbClient = new InfluxDBClient(influxDbOptions.Url, influxDbOptions.Token);
    }

    public async Task PushMetricsAsync()
    {
        if (_influxDbClient == null)
            return;

        _logger.LogDebug("Pushing metrics to InfluxDB.");

        ExportOptions.InfluxDbOptions influxDbOptions = _exportOptions.Value.InfluxDb!;
        WriteApiAsync writeApi = _influxDbClient.GetWriteApiAsync();

        // Use a consistent timestamp for all measurements
        DateTime timestamp = DateTime.UtcNow;

        // Create list of data points
        List<PointData> dataPoints =
            _deviceService.Devices.Select(device => GetDeviceMeasurement(device, timestamp)).ToList();

        // Push data points
        await writeApi.WritePointsAsync(dataPoints, influxDbOptions.Bucket, influxDbOptions.Organisation);
    }

    private PointData GetDeviceMeasurement(IDevice device, DateTime timestamp)
    {
        var measurementAttribute = device.GetType().GetCustomAttribute<InfluxDbMeasurementAttribute>();
        if (measurementAttribute == null)
            throw new Exception($"Device {device.GetType().Name} has no measurement attribute.");

        string measurementName = measurementAttribute.Name;
        PointData measurement = PointData.Measurement(measurementName)
            .Timestamp(timestamp, WritePrecision.S)
            .Tag("device", device.DeviceIdentifier);

        foreach (PropertyInfo property in device.GetType().GetProperties())
        {
            var attribute = property.GetCustomAttribute<InfluxDbMetricAttribute>();
            if (attribute == null)
                continue;

            object? value = property.GetValue(device);
            if (value != null && value.GetType().IsEnum)
                value = value.ToString();

            measurement = measurement.Field(attribute.Field, value);
        }

        return measurement;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _influxDbClient?.Dispose();
    }
}
