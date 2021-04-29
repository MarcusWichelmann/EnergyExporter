using System.ComponentModel.DataAnnotations;

namespace SolarEdgeExporter.Options
{
    public class PollingOptions
    {
        [Range(1, int.MaxValue)]
        public int IntervalSeconds { get; init; } = 5;
    }
}
