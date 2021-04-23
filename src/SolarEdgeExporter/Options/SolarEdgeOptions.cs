using System;
using System.ComponentModel.DataAnnotations;

namespace SolarEdgeExporter.Options
{
    public class SolarEdgeOptions
    {
        [Required]
        public string? Host { get; init; }

        public ushort Port { get; init; }

        [Required]
        public SolarEdgeDevices? Devices { get; init; }

        public class SolarEdgeDevices
        {
            public bool Inverter { get; init; }

            [Range(0, 3)]
            public int Meters { get; init; }

            [Range(0, 3)]
            public int Batteries { get; init; }
        }
    }
}
