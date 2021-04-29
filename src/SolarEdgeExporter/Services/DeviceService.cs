using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SolarEdgeExporter.Devices;
using SolarEdgeExporter.Modbus;
using SolarEdgeExporter.Options;

namespace SolarEdgeExporter.Services
{
    public class DeviceService
    {
        private readonly ILogger<DeviceService> _logger;
        private readonly ModbusReader _modbusReader;
        private readonly IOptions<DevicesOptions> _devicesOptions;

        public IReadOnlyCollection<Inverter> Inverters { get; private set; }
        public IReadOnlyCollection<Meter> Meters { get; private set; }
        public IReadOnlyCollection<Battery> Batteries { get; private set; }

        public DeviceService(ILogger<DeviceService> logger, ModbusReader modbusReader, IOptions<DevicesOptions> devicesOptions)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _modbusReader = modbusReader ?? throw new ArgumentNullException(nameof(modbusReader));
            _devicesOptions = devicesOptions ?? throw new ArgumentNullException(nameof(devicesOptions));
        }

        public async Task ReadDevicesAsync()
        {
            DevicesOptions deviceCounts = _devicesOptions.Value;

            var inverters = new List<Inverter>();
            var meters = new List<Meter>();
            var batteries = new List<Battery>();

            foreach (ushort address in DeviceAddresses.Inverters.Take(deviceCounts.Inverters))
                inverters.Add(await _modbusReader.ReadDeviceAsync<Inverter>(address));
            foreach (ushort address in DeviceAddresses.Meters.Take(deviceCounts.Meters))
                meters.Add(await _modbusReader.ReadDeviceAsync<Meter>(address));
            foreach (ushort address in DeviceAddresses.Batteries.Take(deviceCounts.Batteries))
                batteries.Add(await _modbusReader.ReadDeviceAsync<Battery>(address));

            Inverters = inverters.AsReadOnly();
            Meters = meters.AsReadOnly();
            Batteries = batteries.AsReadOnly();
        }
    }
}
