namespace EnergyExporter.Devices;

public abstract class JanitzaDevice : IDevice
{
    /// <inheritdoc />
    public abstract string DeviceType { get; }

    /// <inheritdoc />
    public abstract string DeviceIdentifier { get; }
}
