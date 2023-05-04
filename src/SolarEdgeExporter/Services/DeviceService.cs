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

namespace SolarEdgeExporter.Services; 

public class DeviceService {
    private readonly ILogger<DeviceService> _logger;

    private readonly IImmutableDictionary<DevicesOptions.ModbusSource, ModbusReader> _modbusReaders;

    private volatile IImmutableList<IDevice> _devices = ImmutableList<IDevice>.Empty;

    public IImmutableList<IDevice> Devices => _devices;

    public DeviceService(
        ILogger<DeviceService> logger,
        IOptions<DevicesOptions> devicesOptions,
        ILoggerFactory loggerFactory) {
        _logger = logger;

        // Create modbus readers
        ILogger<ModbusReader> modbusReaderLogger = loggerFactory.CreateLogger<ModbusReader>();
        _modbusReaders = devicesOptions.Value.ModbusSources!.ToImmutableDictionary(
            source => source,
            source => new ModbusReader(modbusReaderLogger, source.Host!, source.Port, source.Unit));
    }

    public async Task QueryDevicesAsync() {
        // Query all modbus sources concurrently and aggregate the resulting devices
        Task<IReadOnlyCollection<IDevice>>[] queryTasks =
            _modbusReaders.Select(p => QueryModbusSourceAsync(p.Key, p.Value)).ToArray();
        await Task.WhenAll(queryTasks);
        _devices = queryTasks.SelectMany(task => task.Result).ToImmutableList();
    }

    private async Task<IReadOnlyCollection<IDevice>> QueryModbusSourceAsync(
        DevicesOptions.ModbusSource modbusSource,
        ModbusReader modbusReader) {
        _logger.LogDebug("Querying devices from {Endpoint}.", modbusSource.EndpointIdentifier);

        var devices = new List<IDevice>();

        try {
            foreach (ushort address in DeviceAddresses.Inverters.Take(modbusSource.Inverters))
                devices.Add(await modbusReader.ReadDeviceAsync<Inverter>(address));

            _logger.LogDebug("Inverters for {Endpoint} queried successfully!", modbusSource.EndpointIdentifier);
        } catch (Exception ex) {
            _logger.LogError(ex, "Reading inverters from {Endpoint} failed.", modbusSource.EndpointIdentifier);
        }

        try {
            foreach (ushort address in DeviceAddresses.Meters.Take(modbusSource.Meters))
                devices.Add(await modbusReader.ReadDeviceAsync<Meter>(address));

            _logger.LogDebug("Meters for {Endpoint} queried successfully!", modbusSource.EndpointIdentifier);
        } catch (Exception ex) {
            _logger.LogError(ex, "Reading meters from {Endpoint} failed.", modbusSource.EndpointIdentifier);
        }

        try {
            foreach (ushort address in DeviceAddresses.Batteries.Take(modbusSource.Batteries))
                devices.Add(await modbusReader.ReadDeviceAsync<Battery>(address));

            _logger.LogDebug("Batteries for {Endpoint} queried successfully!", modbusSource.EndpointIdentifier);
        } catch (Exception ex) {
            _logger.LogError(ex, "Reading batteries from {Endpoint} failed.", modbusSource.EndpointIdentifier);
        }

        return devices.AsReadOnly();
    }
}
