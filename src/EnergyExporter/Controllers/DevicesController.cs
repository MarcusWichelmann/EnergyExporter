using System.Collections.Generic;
using System.Linq;
using EnergyExporter.Devices;
using EnergyExporter.Services;
using Microsoft.AspNetCore.Mvc;

namespace EnergyExporter.Controllers;

[ApiController]
[Route("devices")]
public class DevicesController : ControllerBase
{
    private readonly DeviceService _deviceService;

    public DevicesController(DeviceService deviceService)
    {
        _deviceService = deviceService;
    }

    [HttpGet]
    public IEnumerable<object> GetDevices()
    {
        // We have to cast to object here, so polymorphism is respected when serializing the objects.
        return _deviceService.Devices.Select(device => (object)device);
    }

    [HttpGet("solaredge")]
    public IEnumerable<object> GetSolarEdgeDevices()
    {
        // We have to cast to object here, so polymorphism is respected when serializing the objects.
        return _deviceService.Devices.OfType<SolarEdgeDevice>().Select(device => (object)device);
    }

    [HttpGet("solaredge/inverters")]
    public IEnumerable<SolarEdgeInverter> GetSolarEdgeInverters() => _deviceService.Devices.OfType<SolarEdgeInverter>();

    [HttpGet("solaredge/inverters/{id}")]
    public ActionResult<SolarEdgeInverter> GetSolarEdgeInverter(string id) =>
        _deviceService.Devices.OfType<SolarEdgeInverter>().FirstOrDefault(d => d.DeviceIdentifier == id)
        ?? (ActionResult<SolarEdgeInverter>)NotFound();

    [HttpGet("solaredge/meters")]
    public IEnumerable<SolarEdgeMeter> GetSolarEdgeMeters() => _deviceService.Devices.OfType<SolarEdgeMeter>();

    [HttpGet("solaredge/meters/{id}")]
    public ActionResult<SolarEdgeMeter> GetSolarEdgeMeter(string id) =>
        _deviceService.Devices.OfType<SolarEdgeMeter>().FirstOrDefault(d => d.DeviceIdentifier == id)
        ?? (ActionResult<SolarEdgeMeter>)NotFound();

    [HttpGet("solaredge/batteries")]
    public IEnumerable<SolarEdgeBattery> GetSolarEdgeBatteries() => _deviceService.Devices.OfType<SolarEdgeBattery>();

    [HttpGet("solaredge/batteries/{id}")]
    public ActionResult<SolarEdgeBattery> GetSolarEdgeBatteries(string id) =>
        _deviceService.Devices.OfType<SolarEdgeBattery>().FirstOrDefault(d => d.DeviceIdentifier == id)
        ?? (ActionResult<SolarEdgeBattery>)NotFound();
    
    [HttpGet("janitza")]
    public IEnumerable<object> GetJanitzaDevices()
    {
        // We have to cast to object here, so polymorphism is respected when serializing the objects.
        return _deviceService.Devices.OfType<JanitzaDevice>().Select(device => (object)device);
    }

    [HttpGet("janitza/power-analyzers")]
    public IEnumerable<JanitzaPowerAnalyzer> GetJanitzaPowerAnalyzers() => _deviceService.Devices.OfType<JanitzaPowerAnalyzer>();

    [HttpGet("janitza/power-analyzers/{id}")]
    public ActionResult<JanitzaPowerAnalyzer> GetJanitzaPowerAnalyzer(string id) =>
        _deviceService.Devices.OfType<JanitzaPowerAnalyzer>().FirstOrDefault(d => d.DeviceIdentifier == id)
        ?? (ActionResult<JanitzaPowerAnalyzer>)NotFound();
}
