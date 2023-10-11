using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace EnergyExporter.Options;

public class DevicesOptions
{
    public const string Key = "Devices";

    public SolarEdgeModbusDevice[] SolarEdge { get; init; } = Array.Empty<SolarEdgeModbusDevice>();
    
    public JanitzaModbusDevice[] Janitza { get; init; } = Array.Empty<JanitzaModbusDevice>();

    public IEnumerable<ModbusDevice> ModbusDevices => SolarEdge.Cast<ModbusDevice>().Concat(Janitza);

    public enum SolarEdgeModbusDeviceType
    {
        Inverter,
        Meter,
        Battery
    }

    public enum JanitzaModbusDeviceType
    {
        PowerAnalyzer
    }
    
    public class SolarEdgeModbusDevice : ModbusDevice
    {
        [Required]
        [EnumDataType(typeof(SolarEdgeModbusDeviceType))]
        public SolarEdgeModbusDeviceType Type { get; init; }

        public byte Index { get; init; }

        public string? SerialNumberOverride { get; init; }
    }

    public class JanitzaModbusDevice : ModbusDevice
    {
        [Required]
        [EnumDataType(typeof(JanitzaModbusDeviceType))]
        public JanitzaModbusDeviceType Type { get; init; }
    }
    
    public class ModbusDevice
    {
        [Required]
        public string Host { get; init; } = null!;

        public ushort Port { get; init; }

        public byte Unit { get; init; }
    }
}
