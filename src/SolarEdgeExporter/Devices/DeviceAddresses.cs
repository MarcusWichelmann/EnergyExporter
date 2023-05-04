namespace SolarEdgeExporter.Devices; 

public static class DeviceAddresses {
    public static readonly ushort[] Inverters = { 0x9C40 };
    public static readonly ushort[] Meters = { 0x9CB9, 0x9D67, 0x9E15 };
    public static readonly ushort[] Batteries = { 0xE100, 0xE200 };
}
