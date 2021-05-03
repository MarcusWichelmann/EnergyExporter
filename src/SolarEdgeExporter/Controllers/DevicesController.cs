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

        [HttpGet("inverters/{index:int}")]
        public ActionResult<Inverter> GetInverter(int index) => _deviceService.Inverters.ElementAtOrDefault(index) ?? (ActionResult<Inverter>)NotFound();

        [HttpGet("meters")]
        public IEnumerable<Meter> GetMeters() => _deviceService.Meters;

        [HttpGet("meters/{index:int}")]
        public ActionResult<Meter> GetMeter(int index) => _deviceService.Meters.ElementAtOrDefault(index) ?? (ActionResult<Meter>)NotFound();

        [HttpGet("batteries")]
        public IEnumerable<Battery> GetBatteries() => _deviceService.Batteries;

        [HttpGet("batteries/{index:int}")]
        public ActionResult<Battery> GetBatteries(int index) => _deviceService.Batteries.ElementAtOrDefault(index) ?? (ActionResult<Battery>)NotFound();
    }
}
