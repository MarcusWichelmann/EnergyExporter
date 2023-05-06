using System.ComponentModel.DataAnnotations;

namespace EnergyExporter.Options; 

public class PollingOptions {
    public const string Key = "Polling";

    [Range(1, int.MaxValue)]
    public int IntervalSeconds { get; init; } = 5;
}
