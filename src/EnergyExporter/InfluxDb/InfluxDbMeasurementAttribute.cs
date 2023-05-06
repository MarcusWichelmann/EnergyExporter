using System;

namespace EnergyExporter.InfluxDb;

/// <summary>
/// Specifies the InfluxDb measurement name for this device.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class InfluxDbMeasurementAttribute : Attribute
{
    public string Name { get; }

    public InfluxDbMeasurementAttribute(string name)
    {
        Name = name;
    }
}
