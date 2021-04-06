using System;
using System.ComponentModel.DataAnnotations;

namespace SolarEdgeExporter.Options
{
    public class SolarEdgeOptions
    {
        [Required]
        public string? Address { get; init; }

        public ushort Port { get; init; }
    }
}
