using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using SolarEdgeExporter.Devices;
using SolarEdgeExporter.Services;

namespace SolarEdgeExporter.Controllers
{
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

        [HttpGet("inverters")]
        public IEnumerable<Inverter> GetInverters() => _deviceService.Devices.OfType<Inverter>();

        [HttpGet("inverters/{id:int}")]
        public ActionResult<Inverter> GetInverter(int id) => _deviceService.Devices.OfType<Inverter>().ElementAtOrDefault(id) ?? (ActionResult<Inverter>)NotFound();

        [HttpGet("meters")]
        public IEnumerable<Meter> GetMeters() => _deviceService.Devices.OfType<Meter>();

        [HttpGet("meters/{id:int}")]
        public ActionResult<Meter> GetMeter(int id) => _deviceService.Devices.OfType<Meter>().ElementAtOrDefault(id) ?? (ActionResult<Meter>)NotFound();

        [HttpGet("batteries")]
        public IEnumerable<Battery> GetBatteries() => _deviceService.Devices.OfType<Battery>();

        [HttpGet("batteries/{id:int}")]
        public ActionResult<Battery> GetBatteries(int id) => _deviceService.Devices.OfType<Battery>().ElementAtOrDefault(id) ?? (ActionResult<Battery>)NotFound();
    }
}
