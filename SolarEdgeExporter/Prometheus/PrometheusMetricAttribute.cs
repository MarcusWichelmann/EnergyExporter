using System;

namespace SolarEdgeExporter.Prometheus
{
    /// <summary>
    /// Specifies how the data for a property gets exported for prometheus.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class PrometheusMetricAttribute : Attribute
    {
        public MetricType Type { get; }
        public string Name { get; }
        public string HelpText { get; }

        public PrometheusMetricAttribute(MetricType type, string name, string helpText)
        {
            Type = type;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            HelpText = helpText ?? throw new ArgumentNullException(nameof(helpText));
        }
    }
}
