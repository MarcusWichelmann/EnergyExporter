using System.Text.Json.Serialization;

namespace EnergyExporter.Devices;

/// <summary>
/// Base class for queried devices.
/// </summary>
public interface IDevice
{
    /// <summary>
    /// A string representing the type of the device.
    /// </summary>
    public string DeviceType { get; }

    /// <summary>
    /// A string used to identify the device in metrics.
    /// </summary>
    [JsonIgnore]
    public string DeviceIdentifier { get; }
}
