using System;
using System.ComponentModel.DataAnnotations;

namespace SolarEdgeExporter.Options
{
    public class ModbusOptions
    {
        [Required]
        public string? Host { get; init; }

        public ushort Port { get; init; }
    }
}
