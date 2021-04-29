using System.ComponentModel.DataAnnotations;

namespace SolarEdgeExporter.Options
{
    public class DevicesOptions
    {
        [Range(0, 1)]
        public int Inverters { get; init; }

        [Range(0, 3)]
        public int Meters { get; init; }

        [Range(0, 2)]
        public int Batteries { get; init; }
    }
}
