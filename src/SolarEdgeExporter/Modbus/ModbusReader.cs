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
        private const byte ModbusUnit = 1;
        private const ushort ReadChunkSize = 64;

        private readonly ILogger<ModbusReader> _logger;
        private readonly IOptions<SolarEdgeOptions> _solarEdgeOptions;

        private readonly ModbusTcpClient _modbusClient = new();

        public ModbusReader(ILogger<ModbusReader> logger, IOptions<SolarEdgeOptions> solarEdgeOptions)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _solarEdgeOptions = solarEdgeOptions ?? throw new ArgumentNullException(nameof(solarEdgeOptions));
        }

        public TDevice ReadDevice<TDevice>(ushort startRegister, ushort registerCount) where TDevice : IDevice
        {
            if (!_modbusClient.IsConnected)
                Reconnect();

            // Read registers chunk by chunk. Each register consists of 2 bytes.
            var registers = new byte[registerCount * 2];
            ushort readCount = 0;
            while (readCount < registerCount)
            {
                ushort chunkSize = Math.Min((ushort)(registerCount - readCount), ReadChunkSize);

                Span<byte> chunkData = _modbusClient.ReadHoldingRegisters(ModbusUnit, (ushort)(startRegister + readCount), chunkSize);
                if (chunkData.Length != chunkSize * 2)
                    throw new ModbusReadException($"Reading registers chunk failed: Expected {chunkSize * 2} bytes but received {chunkData.Length}.");

                chunkData.CopyTo(registers.AsSpan()[(readCount * 2)..]);
                readCount += chunkSize;
            }

            // Instantiate the device
            var device = Activator.CreateInstance<TDevice>();

            // Iterate over device properties
            IEnumerable<PropertyInfo> properties = typeof(TDevice).GetProperties();
            foreach (var property in properties)
            {
                var attribute = Attribute.GetCustomAttribute(property, typeof(ModbusRegisterAttribute));
                if (attribute is not ModbusRegisterAttribute modbusRegisterAttribute)
                    continue;

                // Support enums
                Type propertyType = property.PropertyType;
                if (propertyType.IsEnum)
                    propertyType = propertyType.GetEnumUnderlyingType();

                // Read the register value
                object value = modbusRegisterAttribute.ReadValue(registers, propertyType);

                property.SetValue(device, value);
            }

            return device;
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
    }
}
