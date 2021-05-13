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
            _deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
        }

        [HttpGet("inverters")]
        public IEnumerable<Inverter> GetInverters() => _deviceService.Inverters;

        [HttpGet("inverters/{id:int}")]
        public ActionResult<Inverter> GetInverter(int id) => _deviceService.Inverters.ElementAtOrDefault(id) ?? (ActionResult<Inverter>)NotFound();

        [HttpGet("meters")]
        public IEnumerable<Meter> GetMeters() => _deviceService.Meters;

        [HttpGet("meters/{id:int}")]
        public ActionResult<Meter> GetMeter(int id) => _deviceService.Meters.ElementAtOrDefault(id) ?? (ActionResult<Meter>)NotFound();

        [HttpGet("batteries")]
        public IEnumerable<Battery> GetBatteries() => _deviceService.Batteries;

        [HttpGet("batteries/{id:int}")]
        public ActionResult<Battery> GetBatteries(int id) => _deviceService.Batteries.ElementAtOrDefault(id) ?? (ActionResult<Battery>)NotFound();
    }
}
