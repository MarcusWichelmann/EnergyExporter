using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EnergyExporter.Devices;
using EnergyExporter.Services;
using Microsoft.Extensions.Logging;

namespace EnergyExporter.Prometheus;

public class MetricsWriter
{
    private record MetricsEntry(IDevice Device, PropertyInfo Property, PrometheusMetricAttribute MetricAttribute);

    private readonly ILogger<MetricsWriter> _logger;
    private readonly DeviceService _deviceService;

    public MetricsWriter(ILogger<MetricsWriter> logger, DeviceService deviceService)
    {
        _logger = logger;
        _deviceService = deviceService;
    }

    public async Task WriteToStreamAsync(Stream stream)
    {
        await using var streamWriter = new StreamWriter(stream, new UTF8Encoding(false), -1, true);

        // Aggregate all device metrics based on the attributes on the objects
        IEnumerable<MetricsEntry> metrics = _deviceService.Devices.SelectMany(
            device =>
                from property in device.GetType().GetProperties()
                let metricAttribute = (PrometheusMetricAttribute?)Attribute.GetCustomAttribute(
                    property,
                    typeof(PrometheusMetricAttribute))
                where metricAttribute != null
                select new MetricsEntry(device, property, metricAttribute));

        // Group entries by metric name
        IEnumerable<IGrouping<string, MetricsEntry>> metricGroups =
            metrics.GroupBy(metric => metric.MetricAttribute.Name);

        foreach (IGrouping<string, MetricsEntry> metricGroup in metricGroups)
        {
            PrometheusMetricAttribute metricAttribute = metricGroup.First().MetricAttribute;

            // Check that the metric attributes are not contradictory
            if (metricGroup.Skip(1)
                .Any(
                    m => m.MetricAttribute.Type != metricAttribute.Type
                         || m.MetricAttribute.Description != metricAttribute.Description))
                _logger.LogWarning(
                    "Not all metric attributes for {Metric} have the same type and description.",
                    metricAttribute.Name);

            // Write metric header lines
            await streamWriter.WriteLineAsync(metricAttribute.GetHelpLine());
            await streamWriter.WriteLineAsync(metricAttribute.GetTypeLine());

            // Write sample lines
            foreach (MetricsEntry metricsEntry in metricGroup)
            {
                var value = (double)Convert.ChangeType(
                    metricsEntry.Property.GetValue(metricsEntry.Device)!,
                    typeof(double));

                // Skip values the device reports as "not implemented"
                if (double.IsNaN(value))
                    continue;

                await streamWriter.WriteLineAsync(
                    metricsEntry.MetricAttribute.GetSampleLine(metricsEntry.Device.DeviceIdentifier, value));
            }

            await streamWriter.WriteLineAsync();
        }

        await WriteExporterHealthAsync(streamWriter);
    }

    /// <summary>
    /// Writes metrics about the exporter's own view of each configured device, so outages can be
    /// alerted on and distinguished from the device simply reporting zero.
    /// </summary>
    private async Task WriteExporterHealthAsync(StreamWriter writer)
    {
        IReadOnlyList<DeviceStatus> statuses = _deviceService.GetStatuses();
        if (statuses.Count == 0)
            return;

        static string Labels(DeviceStatus s) =>
            $"{{target=\"{s.Host}:{s.Port}/{s.Unit}\",type=\"{s.Vendor}{s.Type}\",index=\"{s.Index?.ToString() ?? ""}\"}}";

        async Task WriteMetric(string name, string type, string help, Func<DeviceStatus, double> value)
        {
            await writer.WriteLineAsync($"# HELP {name} {help}");
            await writer.WriteLineAsync($"# TYPE {name} {type}");
            foreach (DeviceStatus status in statuses)
                await writer.WriteLineAsync(
                    $"{name}{Labels(status)} {value(status).ToString(CultureInfo.InvariantCulture)}");
            await writer.WriteLineAsync();
        }

        await WriteMetric(
            "energy_exporter_device_up",
            "gauge",
            "Whether the last poll of the device succeeded (1) or failed (0)",
            s => s.Online ? 1 : 0);
        await WriteMetric(
            "energy_exporter_device_polls_total",
            "counter",
            "Number of polls of the device",
            s => s.TotalPolls);
        await WriteMetric(
            "energy_exporter_device_poll_failures_total",
            "counter",
            "Number of failed polls of the device",
            s => s.TotalFailures);
        await WriteMetric(
            "energy_exporter_device_poll_duration_seconds",
            "gauge",
            "Duration of the last poll of the device",
            s => s.LastDurationMs / 1000);
    }
}
