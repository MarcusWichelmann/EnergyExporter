using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SolarEdgeExporter.Prometheus
{
    /// <summary>
    /// Specifies that the value of this property should be exported for prometheus.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class PrometheusMetricAttribute : Attribute
    {
        public MetricType Type { get; }

        public string Name { get; }

        public string Description { get; }

        public PrometheusMetricAttribute(MetricType type, string name, string description)
        {
            if (!Enum.IsDefined(typeof(MetricType), type))
                throw new InvalidEnumArgumentException(nameof(type), (int)type, typeof(MetricType));

            Type = type;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description ?? throw new ArgumentNullException(nameof(description));
        }

        public string GetHelpLine() => $"# HELP {Name} {Description}";

        public string GetTypeLine() => $"# TYPE {Name} {Type.ToTypeName()}";

        public string GetSampleLine(string deviceName, int deviceId, double propertyValue)
        {
            if (string.IsNullOrWhiteSpace(deviceName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(deviceName));

            return $"{Name}{{{deviceName}=\"{deviceId}\"}} {propertyValue}";
        }
    }
}
