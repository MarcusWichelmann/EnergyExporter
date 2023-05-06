using System;
using System.ComponentModel;
using System.Globalization;

namespace EnergyExporter.Prometheus; 

/// <summary>
/// Specifies that the value of this property should be exported for prometheus.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class PrometheusMetricAttribute : Attribute {
    public MetricType Type { get; }

    public string Name { get; }

    public string Description { get; }

    public PrometheusMetricAttribute(MetricType type, string name, string description) {
        if (!Enum.IsDefined(typeof(MetricType), type))
            throw new InvalidEnumArgumentException(nameof(type), (int)type, typeof(MetricType));

        Type = type;
        Name = name;
        Description = description;
    }

    public string GetHelpLine() => $"# HELP {Name} {Description}";

    public string GetTypeLine() => $"# TYPE {Name} {Type.ToTypeName()}";

    public string GetSampleLine(string deviceIdentifier, double propertyValue) {
        if (string.IsNullOrWhiteSpace(deviceIdentifier))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(deviceIdentifier));

        return $"{Name}{{device=\"{deviceIdentifier}\"}} {propertyValue.ToString(CultureInfo.InvariantCulture)}";
    }
}
