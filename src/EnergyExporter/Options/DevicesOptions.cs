using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

using EnergyExporter.Devices;

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
    
    public class SolarEdgeModbusDevice : ModbusDevice, IValidatableObject
    {
        [Required]
        [EnumDataType(typeof(SolarEdgeModbusDeviceType))]
        public SolarEdgeModbusDeviceType Type { get; init; }

        public byte Index { get; init; }

        public string? SerialNumberOverride { get; init; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            int maxIndex = Type switch {
                SolarEdgeModbusDeviceType.Meter => SolarEdgeMeter.ModbusAddresses.Length - 1,
                SolarEdgeModbusDeviceType.Battery => SolarEdgeBattery.ModbusAddresses.Length - 1,
                _ => 0
            };
            if (Index > maxIndex)
                yield return new ValidationResult(
                    $"Index {Index} is out of range for a SolarEdge {Type} at {Host}; it must be between 0 and {maxIndex}.",
                    new[] { nameof(Index) });
        }
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

        [Range(1, 65535)]
        public ushort Port { get; init; }

        public byte Unit { get; init; }
    }
}
