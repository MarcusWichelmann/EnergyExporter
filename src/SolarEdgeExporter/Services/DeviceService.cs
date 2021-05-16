using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

        private volatile IImmutableList<Inverter> _inverters = ImmutableList<Inverter>.Empty;
        private volatile IImmutableList<Meter> _meters = ImmutableList<Meter>.Empty;
        private volatile IImmutableList<Battery> _batteries = ImmutableList<Battery>.Empty;

        public IImmutableList<Inverter> Inverters => _inverters;
        public IImmutableList<Meter> Meters => _meters;
        public IImmutableList<Battery> Batteries => _batteries;

        public DeviceService(ILogger<DeviceService> logger, ModbusReader modbusReader,
            IOptions<DevicesOptions> devicesOptions)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _modbusReader = modbusReader ?? throw new ArgumentNullException(nameof(modbusReader));
            _devicesOptions = devicesOptions ?? throw new ArgumentNullException(nameof(devicesOptions));
        }

        public async Task ReadDevicesAsync()
        {
            DevicesOptions deviceCounts = _devicesOptions.Value;

            // Try to read the devices by type and make sure the stored device data is cleared when a read fails
            // so no outdated data is exported.

            ImmutableList<Inverter>.Builder inverters = ImmutableList.CreateBuilder<Inverter>();
            try
            {
                foreach (ushort address in DeviceAddresses.Inverters.Take(deviceCounts.Inverters))
                    inverters.Add(await _modbusReader.ReadDeviceAsync<Inverter>(address));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Reading inverters failed.");
            }

            ImmutableList<Meter>.Builder meters = ImmutableList.CreateBuilder<Meter>();
            try
            {
                foreach (ushort address in DeviceAddresses.Meters.Take(deviceCounts.Meters))
                    meters.Add(await _modbusReader.ReadDeviceAsync<Meter>(address));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Reading meters failed.");
            }

            ImmutableList<Battery>.Builder batteries = ImmutableList.CreateBuilder<Battery>();
            try
            {
                foreach (ushort address in DeviceAddresses.Batteries.Take(deviceCounts.Batteries))
                    batteries.Add(await _modbusReader.ReadDeviceAsync<Battery>(address));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Reading batteries failed.");
            }

            _inverters = inverters.ToImmutable();
            _meters = meters.ToImmutable();
            _batteries = batteries.ToImmutable();
        }
    }
}
