using System.ComponentModel.DataAnnotations;

namespace SolarEdgeExporter.Options; 

public class DevicesOptions {
    public const string Key = "Devices";

    [Required]
    [MinLength(1)]
    public ModbusSource[]? ModbusSources { get; init; }

    public class ModbusSource {
        [Required]
        public string? Host { get; init; }

        public ushort Port { get; init; }

        public byte Unit { get; init; }

        [Range(0, 1)]
        public int Inverters { get; init; }

        [Range(0, 3)]
        public int Meters { get; init; }

        [Range(0, 2)]
        public int Batteries { get; init; }

        public string EndpointIdentifier => $"{Host}:{Port}#{Unit}";
    }
}
