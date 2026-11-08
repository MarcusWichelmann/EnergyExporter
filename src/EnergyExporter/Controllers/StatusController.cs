using System;
using System.Collections.Generic;
using System.Linq;
using EnergyExporter.Modbus;
using EnergyExporter.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EnergyExporter.Controllers;

/// <summary>
/// Backs the management UI: exporter health, per-device status and simple controls.
/// </summary>
[ApiController]
[Route("api")]
public class StatusController : ControllerBase
{
    private readonly DeviceService _deviceService;
    private readonly DevicePollingService _pollingService;
    private readonly ModbusReaderPool _readerPool;
    private readonly StatusEventLog _eventLog;

    public StatusController(
        DeviceService deviceService,
        DevicePollingService pollingService,
        ModbusReaderPool readerPool,
        StatusEventLog eventLog)
    {
        _deviceService = deviceService;
        _pollingService = pollingService;
        _readerPool = readerPool;
        _eventLog = eventLog;
    }

    public record ConnectionInfo(string Endpoint, bool Connected);

    public record DeviceStatusDto(DeviceStatus Status, object? LastReading);

    public record StatusDto(
        string Version,
        DateTimeOffset StartedAt,
        double PollingIntervalSeconds,
        bool IsPolling,
        DateTimeOffset? LastPoll,
        DateTimeOffset? NextPoll,
        IEnumerable<DeviceStatusDto> Devices,
        IEnumerable<ConnectionInfo> Connections,
        IEnumerable<ExporterStatus> Exporters,
        IEnumerable<StatusEvent> Events);

    [HttpGet("status")]
    public StatusDto GetStatus() =>
        new(
            typeof(StatusController).Assembly.GetName().Version?.ToString() ?? "unknown",
            _pollingService.StartedAt,
            _pollingService.Interval.TotalSeconds,
            _pollingService.IsPolling,
            _deviceService.LastPoll,
            _pollingService.NextPoll,
            // Cast the reading to object, so polymorphic serialization includes all device properties.
            _deviceService.GetStatuses().Select(s => new DeviceStatusDto(s, s.LastReading)),
            _readerPool.Readers.Select(r => new ConnectionInfo(r.Endpoint, r.IsConnected)),
            _pollingService.ExporterStatuses.OrderBy(e => e.Name),
            _eventLog.Recent);

    /// <summary>
    /// Polls all devices immediately.
    /// </summary>
    [HttpPost("poll")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public IActionResult PollNow()
    {
        _pollingService.PollNow();
        return Accepted();
    }

    /// <summary>
    /// Drops the modbus connection of every device; the next poll reconnects.
    /// </summary>
    [HttpPost("reconnect")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public IActionResult ReconnectAll()
    {
        _deviceService.RequestReconnect();
        _pollingService.PollNow();
        return Accepted();
    }

    /// <summary>
    /// Drops the modbus connection used by one device; the next poll reconnects.
    /// </summary>
    [HttpPost("devices/{id:int}/reconnect")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Reconnect(int id)
    {
        if (!_deviceService.RequestReconnect(id))
            return NotFound();
        _pollingService.PollNow();
        return Accepted();
    }
}
