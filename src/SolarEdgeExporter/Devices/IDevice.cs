using System.Text.Json.Serialization;

namespace SolarEdgeExporter.Devices; 

/// <summary>
/// Base class for devices of the solar installation.
/// </summary>
public interface IDevice {
    /// <summary>
    /// A string used to identify the device in metrics.
    /// </summary>
    [JsonIgnore]
    public string DeviceIdentifier { get; }
}
