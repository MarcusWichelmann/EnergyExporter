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

            // Read registers. Each register consists of 2 bytes.
            var registers = _modbusClient.ReadHoldingRegisters(ModbusUnit, startRegister, registerCount);
            if (registers.Length != registerCount * 2)
                throw new ModbusReadException(
                    $"Reading registers failed: Expected {registerCount * 2} bytes but received {registers.Length}.");

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
                var value = modbusRegisterAttribute.ReadValue(registers, property.PropertyType);
                
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
