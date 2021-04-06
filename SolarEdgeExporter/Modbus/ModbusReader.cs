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

        private readonly ILogger<ModbusReader> _logger;
        private readonly IOptions<SolarEdgeOptions> _solarEdgeOptions;

        private readonly ModbusTcpClient _modbusClient = new ModbusTcpClient();

        public ModbusReader(ILogger<ModbusReader> logger, IOptions<SolarEdgeOptions> solarEdgeOptions)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _solarEdgeOptions = solarEdgeOptions ?? throw new ArgumentNullException(nameof(solarEdgeOptions));
        }

        public TDevice ReadDevice<TDevice>(ushort startRegister, ushort registerCount) where TDevice : IDevice
        {
            if (!_modbusClient.IsConnected)
                Reconnect();

            // Read registers. Each register consists of 2 bytes.
            Span<byte> data = _modbusClient.ReadHoldingRegisters(ModbusUnit, startRegister, registerCount);
            if (data.Length != registerCount * 2)
                throw new ModbusReadException(
                    $"Reading registers failed: Expected {registerCount * 2} bytes but received {data.Length}.");

            // Instantiate the device
            var device = Activator.CreateInstance<TDevice>();

            // Iterate over device properties
            IEnumerable<PropertyInfo> properties = typeof(TDevice).GetProperties();
            foreach (var property in properties)
            {
                var attribute = Attribute.GetCustomAttribute(property, typeof(ModbusRegisterAttribute));
                if (attribute is not ModbusRegisterAttribute modbusRegisterAttribute)
                    continue;

                int offsetBytes = modbusRegisterAttribute.RelativeRegister * 2;

                // TODO: Extract methods

                object value;
                switch (attribute)
                {
                    case StringModbusRegisterAttribute stringModbusRegisterAttribute:
                        value = Encoding.UTF8.GetString(data.Slice(offsetBytes, stringModbusRegisterAttribute.Length))
                            .TrimEnd('\0', ' ');
                        break;

                    case ScaledModbusRegisterAttribute scaledModbusRegisterAttribute:

                        value = 1;
                        break;
                    default:
                        value = 1;
                        break;
                }

                property.SetValue(device, value);
            }

            return device;
        }

        private void Reconnect()
        {
            _logger.LogInformation("Connecting to modbus server...");

            string? addressString = _solarEdgeOptions.Value.Address;
            if (!IPAddress.TryParse(addressString, out IPAddress? address))
                throw new ModbusReadException($"Invalid IP address: {addressString}");

            var endpoint = new IPEndPoint(address, _solarEdgeOptions.Value.Port);

            _modbusClient.ReadTimeout = 5 * 1000;
            _modbusClient.Connect(endpoint);
        }
    }
}
