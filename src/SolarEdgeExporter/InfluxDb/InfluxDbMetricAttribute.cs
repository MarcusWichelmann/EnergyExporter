using System;

namespace SolarEdgeExporter.InfluxDb;

/// <summary>
/// Specifies that the value of this property should be exported to InfluxDb.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class InfluxDbMetricAttribute : Attribute {
    public string Field { get; }

    public InfluxDbMetricAttribute(string field) {
        Field = field;
    }
}
