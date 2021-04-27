using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using FluentModbus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SolarEdgeExporter.Devices;
using SolarEdgeExporter.Options;

namespace SolarEdgeExporter.Modbus
{
    public class ModbusReader
    {
        public const byte ModbusUnit = 1;

        private readonly ILogger<ModbusReader> _logger;
        private readonly IOptions<SolarEdgeOptions> _solarEdgeOptions;

        private readonly ModbusTcpClient _modbusClient = new();

        public ModbusReader(ILogger<ModbusReader> logger, IOptions<SolarEdgeOptions> solarEdgeOptions)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _solarEdgeOptions = solarEdgeOptions ?? throw new ArgumentNullException(nameof(solarEdgeOptions));
        }

        public async Task<TDevice> ReadDeviceAsync<TDevice>(ushort startRegister) where TDevice : IDevice
        {
            // Ensure the client is connected
            if (!_modbusClient.IsConnected)
                Reconnect();

            // Create a list of all relative register addresses that need to be read
            ushort[] relativeAddressesToRead = typeof(TDevice).GetProperties().SelectMany(prop => {
                var attribute = Attribute.GetCustomAttribute(prop, typeof(ModbusRegisterAttribute));
                return attribute is not ModbusRegisterAttribute modbusRegisterAttribute
                    ? Enumerable.Empty<ushort>()
                    : modbusRegisterAttribute.GetRelativeAddressesToRead(prop.PropertyType);
            }).OrderBy(r => r).Distinct().ToArray();

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
                    Memory<byte> chunkData = await _modbusClient.ReadHoldingRegistersAsync(ModbusUnit, (ushort)(startRegister + chunkStart), chunkSize);
                    if (chunkData.Length != chunkSize * ModbusUtils.SingleRegisterSize)
                        throw new ModbusReadException($"Reading registers chunk failed: Expected {chunkSize * 2} bytes but received {chunkData.Length}.");
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

        private void Reconnect()
        {
            _logger.LogInformation("Connecting to modbus server...");

            string? addressString = _solarEdgeOptions.Value.Host;
            if (!IPAddress.TryParse(addressString, out IPAddress? address))
                throw new ModbusReadException($"Invalid IP address: {addressString}");

            var endpoint = new IPEndPoint(address, _solarEdgeOptions.Value.Port);

            _modbusClient.ReadTimeout = 5000;
            _modbusClient.Connect(endpoint);
        }

        private TDevice CreateDeviceInstance<TDevice>(ReadOnlySpan<byte> registers) where TDevice : IDevice
        {
            // Instantiate the device
            var device = Activator.CreateInstance<TDevice>();

            // Iterate over device properties
            IEnumerable<PropertyInfo> properties = typeof(TDevice).GetProperties();
            foreach (var property in properties)
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
}
