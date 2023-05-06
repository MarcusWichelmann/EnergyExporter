using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EnergyExporter.Options;

public class DevicesOptions
{
    public const string Key = "Devices";

    public SolarEdgeModbusDevice[] SolarEdge { get; init; } = Array.Empty<SolarEdgeModbusDevice>();

    public IEnumerable<ModbusDevice> ModbusDevices => SolarEdge;

    public enum SolarEdgeModbusDeviceType
    {
        Inverter,
        Meter,
        Battery
    }

    public class SolarEdgeModbusDevice : ModbusDevice
    {
        [Required]
        [EnumDataType(typeof(SolarEdgeModbusDeviceType))]
        public SolarEdgeModbusDeviceType Type { get; init; }

        public byte Index { get; init; }

        public string? SerialNumberOverride { get; init; }
    }

    public class ModbusDevice
    {
        [Required]
        public string Host { get; init; } = null!;

        public ushort Port { get; init; }

        public byte Unit { get; init; }
    }
}
