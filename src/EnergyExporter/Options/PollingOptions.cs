using System.ComponentModel.DataAnnotations;

namespace EnergyExporter.Options;

public class PollingOptions
{
    public const string Key = "Polling";

    [Range(1, int.MaxValue)]
    public int IntervalSeconds { get; init; } = 5;

    /// <summary>
    /// How long to wait for a TCP connection to a modbus device before giving up.
    /// </summary>
    [Range(1, 120)]
    public int ConnectTimeoutSeconds { get; init; } = 5;

    /// <summary>
    /// How long to wait for a single modbus response before giving up.
    /// </summary>
    [Range(1, 120)]
    public int ReadTimeoutSeconds { get; init; } = 5;

    /// <summary>
    /// How often a failed device read is retried (with a fresh connection) within the same poll.
    /// </summary>
    [Range(0, 5)]
    public int Retries { get; init; } = 1;
}
