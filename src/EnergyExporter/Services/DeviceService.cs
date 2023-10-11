using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using EnergyExporter.Devices;
using EnergyExporter.Modbus;
using EnergyExporter.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EnergyExporter.Services;

public class DeviceService
{
    private readonly ILogger<DeviceService> _logger;
    private readonly IOptions<DevicesOptions> _devicesOptions;
    private readonly ModbusReaderPool _modbusReaderPool;

    private IImmutableList<IDevice> _devices = ImmutableList<IDevice>.Empty;
    private readonly object _devicesLock = new();

    public IImmutableList<IDevice> Devices
    {
        get
        {
            lock (_devicesLock)
            {
                return _devices;
            }
        }
    }

    public DeviceService(
        ILogger<DeviceService> logger,
        IOptions<DevicesOptions> devicesOptions,
        ModbusReaderPool modbusReaderPool)
    {
        _logger = logger;
        _devicesOptions = devicesOptions;
        _modbusReaderPool = modbusReaderPool;
    }

    public async Task QueryDevicesAsync()
    {
        _logger.LogDebug("Querying devices.");

        IDevice?[] devices = await Task.WhenAll(_devicesOptions.Value.ModbusDevices.Select(QueryModbusDeviceAsync));
        lock (_devicesLock)
        {
            _devices = devices.Where(d => d != null).Cast<IDevice>().ToImmutableList();
        }
    }

    private async Task<IDevice?> QueryModbusDeviceAsync(DevicesOptions.ModbusDevice modbusDevice)
    {
        try
        {
            return modbusDevice switch {
                DevicesOptions.SolarEdgeModbusDevice solarEdgeModbusDevice => await QuerySolarEdgeModbusDeviceAsync(
                    solarEdgeModbusDevice),
                DevicesOptions.JanitzaModbusDevice janitzaModbusDevice => await QueryJanitzaModbusDeviceAsync(
                    janitzaModbusDevice),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Querying modbus device {DeviceType} failed.", modbusDevice.GetType());
            return null;
        }
    }

    private async Task<IDevice> QuerySolarEdgeModbusDeviceAsync(DevicesOptions.SolarEdgeModbusDevice modbusDevice)
    {
        ModbusReader modbusReader = _modbusReaderPool.ReaderFor(modbusDevice.Host, modbusDevice.Port);

        SolarEdgeDevice device = modbusDevice.Type switch {
            DevicesOptions.SolarEdgeModbusDeviceType.Inverter => await modbusReader.ReadDeviceAsync<SolarEdgeInverter>(
                modbusDevice.Unit,
                SolarEdgeInverter.ModbusAddress),
            DevicesOptions.SolarEdgeModbusDeviceType.Meter => await modbusReader.ReadDeviceAsync<SolarEdgeMeter>(
                modbusDevice.Unit,
                SolarEdgeMeter.ModbusAddresses[modbusDevice.Index]),
            DevicesOptions.SolarEdgeModbusDeviceType.Battery => await modbusReader.ReadDeviceAsync<SolarEdgeBattery>(
                modbusDevice.Unit,
                SolarEdgeBattery.ModbusAddresses[modbusDevice.Index]),
            _ => throw new ArgumentOutOfRangeException()
        };

        if (modbusDevice.SerialNumberOverride != null)
            device.SerialNumber = modbusDevice.SerialNumberOverride;

        return device;
    }

    private async Task<IDevice> QueryJanitzaModbusDeviceAsync(DevicesOptions.JanitzaModbusDevice modbusDevice)
    {
        ModbusReader modbusReader = _modbusReaderPool.ReaderFor(modbusDevice.Host, modbusDevice.Port);

        JanitzaDevice device = modbusDevice.Type switch
        {
            DevicesOptions.JanitzaModbusDeviceType.PowerAnalyzer => await modbusReader
                .ReadDeviceAsync<JanitzaPowerAnalyzer>(
                    modbusDevice.Unit,
                    JanitzaPowerAnalyzer.ModbusAddress),
            _ => throw new ArgumentOutOfRangeException()
        };
            
        return device;
    }
}
