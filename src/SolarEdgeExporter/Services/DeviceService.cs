using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SolarEdgeExporter.Devices;
using SolarEdgeExporter.Modbus;
using SolarEdgeExporter.Options;

namespace SolarEdgeExporter.Services
{
    public class DeviceService
    {
        private readonly ModbusReader _modbusReader;
        private readonly IOptions<DevicesOptions> _devicesOptions;

        private volatile IImmutableList<Inverter> _inverters = ImmutableList<Inverter>.Empty;
        private volatile IImmutableList<Meter> _meters = ImmutableList<Meter>.Empty;
        private volatile IImmutableList<Battery> _batteries = ImmutableList<Battery>.Empty;

        public IImmutableList<Inverter> Inverters => _inverters;
        public IImmutableList<Meter> Meters => _meters;
        public IImmutableList<Battery> Batteries => _batteries;

        public DeviceService(ModbusReader modbusReader, IOptions<DevicesOptions> devicesOptions)
        {
            _modbusReader = modbusReader ?? throw new ArgumentNullException(nameof(modbusReader));
            _devicesOptions = devicesOptions ?? throw new ArgumentNullException(nameof(devicesOptions));
        }

        public async Task ReadDevicesAsync()
        {
            DevicesOptions deviceCounts = _devicesOptions.Value;

            ImmutableList<Inverter>.Builder inverters = ImmutableList.CreateBuilder<Inverter>();
            foreach (ushort address in DeviceAddresses.Inverters.Take(deviceCounts.Inverters))
                inverters.Add(await _modbusReader.ReadDeviceAsync<Inverter>(address));
            _inverters = inverters.ToImmutable();

            ImmutableList<Meter>.Builder meters = ImmutableList.CreateBuilder<Meter>();
            foreach (ushort address in DeviceAddresses.Meters.Take(deviceCounts.Meters))
                meters.Add(await _modbusReader.ReadDeviceAsync<Meter>(address));
            _meters = meters.ToImmutable();

            ImmutableList<Battery>.Builder batteries = ImmutableList.CreateBuilder<Battery>();
            foreach (ushort address in DeviceAddresses.Batteries.Take(deviceCounts.Batteries))
                batteries.Add(await _modbusReader.ReadDeviceAsync<Battery>(address));
            _batteries = batteries.ToImmutable();
        }
    }
}
