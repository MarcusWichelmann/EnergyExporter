using System.ComponentModel.DataAnnotations;

namespace SolarEdgeExporter.Options
{
    public class ModbusOptions
    {
        public const string Key = "Modbus";

        [Required]
        public string? Host { get; init; }

        public ushort Port { get; init; }
    }
}
