using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using EnergyExporter.Devices;
using FluentModbus;
using Microsoft.Extensions.Logging;

namespace EnergyExporter.Modbus;

public class ModbusReader
{
    private readonly ILogger<ModbusReader> _logger;

    private readonly string _host;
    private readonly ushort _port;

    private readonly ModbusTcpClient _modbusClient = new();
    private readonly SemaphoreSlim _modbusLock = new(1);

    public ModbusReader(ILogger<ModbusReader> logger, string host, ushort port)
    {
        _logger = logger;
        _host = host;
        _port = port;
    }

    public async Task<TDevice> ReadDeviceAsync<TDevice>(byte unit, ushort startRegister) where TDevice : IDevice
    {
        await _modbusLock.WaitAsync();
        try
        {
            // Ensure the client is connected
            if (!_modbusClient.IsConnected)
                Reconnect();

            _logger.LogDebug(
                "Reading {Device} at address {StartRegister} from {Host} and unit {Unit}.",
                typeof(TDevice).Name,
                $"0x{startRegister}",
                _host,
                unit);

            // Create a list of all relative register addresses that need to be read
            ushort[] relativeAddressesToRead = typeof(TDevice).GetProperties()
                .SelectMany(
                    prop => {
                        var attribute = Attribute.GetCustomAttribute(prop, typeof(ModbusRegisterAttribute));
                        return attribute is not ModbusRegisterAttribute modbusRegisterAttribute
                            ? Enumerable.Empty<ushort>()
                            : modbusRegisterAttribute.GetRelativeAddressesToRead(prop.PropertyType);
                    })
                .OrderBy(r => r)
                .Distinct()
                .ToArray();

            if (relativeAddressesToRead.Length == 0)
                throw new ModbusReadException("Could not find any register addresses to read.");

            int registerCount = relativeAddressesToRead.Max() + 1;
            Memory<byte> data = new byte[registerCount * ModbusUtils.SingleRegisterSize];

            try
            {
                // Read the required registers in as large chunks as possible
                ushort chunkStart = relativeAddressesToRead.First();
                ushort chunkEnd = chunkStart;

                for (var i = 1; i < relativeAddressesToRead.Length; i++)
                {
                    ushort relativeAddress = relativeAddressesToRead[i];

                    // Continue until the next gap
                    if (chunkEnd + 1 == relativeAddress)
                    {
                        chunkEnd = relativeAddress;

                        // Will more registers follow?
                        if (i < relativeAddressesToRead.Length - 1)
                            continue;
                    }

                    // Read a chunk of registers
                    var chunkSize = (ushort)(chunkEnd - chunkStart + 1);
                    Memory<byte> chunkData = await _modbusClient.ReadHoldingRegistersAsync(
                        unit,
                        (ushort)(startRegister + chunkStart),
                        chunkSize);
                    if (chunkData.Length != chunkSize * ModbusUtils.SingleRegisterSize)
                        throw new ModbusReadException(
                            $"Reading registers chunk failed: Expected {chunkSize * 2} bytes but received {chunkData.Length}.");
                    chunkData.CopyTo(data[(chunkStart * ModbusUtils.SingleRegisterSize)..]);

                    // Skip the gap and read the next chunk
                    chunkStart = chunkEnd = relativeAddress;
                }
            }
            catch
            {
                // Make sure the connection gets reestablished after a failed read, just in case...
                _modbusClient.Disconnect();
                throw;
            }

            return CreateDeviceInstance<TDevice>(data.Span);
        }
        finally
        {
            _modbusLock.Release();
        }
    }

    private void Reconnect()
    {
        _logger.LogInformation("Connecting to modbus server at {Host}.", _host);

        if (!IPAddress.TryParse(_host, out IPAddress? address))
            throw new ModbusReadException($"Invalid IP address: {_host}");

        var endpoint = new IPEndPoint(address, _port);

        _modbusClient.ReadTimeout = 5000;
        _modbusClient.Connect(endpoint);

        _logger.LogInformation("Modbus connection to {Host} established.", _host);
    }

    private TDevice CreateDeviceInstance<TDevice>(ReadOnlySpan<byte> registers) where TDevice : IDevice
    {
        // Instantiate the device
        var device = Activator.CreateInstance<TDevice>();

        // Iterate over device properties
        IEnumerable<PropertyInfo> properties = typeof(TDevice).GetProperties();
        foreach (PropertyInfo? property in properties)
        {
            var attribute = Attribute.GetCustomAttribute(property, typeof(ModbusRegisterAttribute));
            if (attribute is not ModbusRegisterAttribute modbusRegisterAttribute)
                continue;

            // Read the register value
            object value = modbusRegisterAttribute.Read(registers, property.PropertyType);

            property.SetValue(device, value);
        }

        return device;
    }
}
