using System;
using System.ComponentModel;

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
        public string Description { get; }
        public object Labels { get; }

        public PrometheusMetricAttribute(MetricType type, string name, string description, object labels)
        {
            if (!Enum.IsDefined(typeof(MetricType), type))
                throw new InvalidEnumArgumentException(nameof(type), (int)type, typeof(MetricType));

            Type = type;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description ?? throw new ArgumentNullException(nameof(description));
            Labels = labels ?? throw new ArgumentNullException(nameof(labels));
        }
    }
}
