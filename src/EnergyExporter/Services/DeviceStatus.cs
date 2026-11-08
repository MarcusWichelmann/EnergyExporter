using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using EnergyExporter.Devices;

namespace EnergyExporter.Services;

/// <summary>
/// Health information about one configured device. Exists even while the device cannot be reached.
/// </summary>
public class DeviceStatus
{
    public required int Id { get; init; }

    /// <summary>
    /// Human readable description of the configured device, e.g. "SolarEdge Meter #0".
    /// </summary>
    public required string Name { get; init; }

    public required string Vendor { get; init; }

    public required string Type { get; init; }

    public required string Host { get; init; }

    public required ushort Port { get; init; }

    public required byte Unit { get; init; }

    public byte? Index { get; init; }

    public bool Online { get; set; }

    public DateTimeOffset? LastAttempt { get; set; }

    public DateTimeOffset? LastSuccess { get; set; }

    public DateTimeOffset? LastFailure { get; set; }

    public DateTimeOffset? OfflineSince { get; set; }

    public string? LastError { get; set; }

    public int ConsecutiveFailures { get; set; }

    public long TotalPolls { get; set; }

    public long TotalFailures { get; set; }

    public double LastDurationMs { get; set; }

    /// <summary>
    /// The most recent successful reading. Kept while the device is offline, so the UI can show
    /// the last known values; it is not exported as metrics once the device fails.
    /// </summary>
    [JsonIgnore]
    public IDevice? LastReading { get; set; }

    public DeviceStatus Snapshot() => (DeviceStatus)MemberwiseClone();
}

public record StatusEvent(DateTimeOffset Timestamp, string Level, string Source, string Message);

public record ExporterStatus(
    string Name,
    bool Enabled,
    bool? Healthy,
    DateTimeOffset? LastSuccess,
    DateTimeOffset? LastFailure,
    string? LastError);

/// <summary>
/// A small in-memory log of noteworthy events (devices going on/offline, exporter failures) for the UI.
/// </summary>
public class StatusEventLog
{
    private const int Capacity = 100;

    private readonly LinkedList<StatusEvent> _events = new();
    private readonly object _lock = new();

    public void Add(string level, string source, string message)
    {
        lock (_lock)
        {
            _events.AddFirst(new StatusEvent(DateTimeOffset.UtcNow, level, source, message));
            while (_events.Count > Capacity)
                _events.RemoveLast();
        }
    }

    public IReadOnlyList<StatusEvent> Recent
    {
        get
        {
            lock (_lock)
            {
                return new List<StatusEvent>(_events);
            }
        }
    }
}
