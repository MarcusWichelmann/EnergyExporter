using System.Text.Json.Serialization;

namespace EnergyExporter.Devices;

public abstract class SolarEdgeDevice : IDevice
{
    /// <summary>
    /// Because SolarEdge has a bug and sometimes reports incorrect SerialNumbers,
    /// this property is abstracted to allow us to override it.
    /// </summary>
    public abstract string? SerialNumber { get; set; }

    /// <inheritdoc />
    public abstract string DeviceType { get; }

    /// <inheritdoc />
    [JsonIgnore]
    public string DeviceIdentifier => SerialNumber!;
}
