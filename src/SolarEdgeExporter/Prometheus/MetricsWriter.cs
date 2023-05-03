using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SolarEdgeExporter.Devices;
using SolarEdgeExporter.Services;

namespace SolarEdgeExporter.Prometheus
{
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
            IEnumerable<MetricsEntry> metrics = _deviceService.Devices.SelectMany(device =>
                from property in device.GetType().GetProperties()
                let metricAttribute = (PrometheusMetricAttribute?)Attribute.GetCustomAttribute(property, typeof(PrometheusMetricAttribute))
                where metricAttribute != null
                select new MetricsEntry(device, property, metricAttribute));

            // Group entries by metric name
            IEnumerable<IGrouping<string, MetricsEntry>> metricGroups = metrics.GroupBy(metric => metric.MetricAttribute.Name);

            foreach (IGrouping<string, MetricsEntry> metricGroup in metricGroups)
            {
                PrometheusMetricAttribute metricAttribute = metricGroup.First().MetricAttribute;

                // Check that the metric attributes are not contradictory
                if (metricGroup.Skip(1).Any(m => m.MetricAttribute.Type != metricAttribute.Type || m.MetricAttribute.Description != metricAttribute.Description))
                    _logger.LogWarning($"Not all metric attributes for \"{metricAttribute.Name}\" have the same type and description.");

                // Write metric header lines
                await streamWriter.WriteLineAsync(metricAttribute.GetHelpLine());
                await streamWriter.WriteLineAsync(metricAttribute.GetTypeLine());

                // Write sample lines
                foreach (MetricsEntry metricsEntry in metricGroup)
                {
                    var value = (double)Convert.ChangeType(metricsEntry.Property.GetValue(metricsEntry.Device)!, typeof(double));
                    await streamWriter.WriteLineAsync(metricsEntry.MetricAttribute.GetSampleLine(metricsEntry.Device.DeviceIdentifier, value));
                }

                await streamWriter.WriteLineAsync();
            }
        }
    }
}
