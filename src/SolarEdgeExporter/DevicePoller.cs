using System;
using Microsoft.Extensions.Logging;
using SolarEdgeExporter.Modbus;

namespace SolarEdgeExporter
{
    public class DevicePoller
    {
        private readonly ILogger<DevicePoller> _logger;
        private readonly ModbusReader _modbusReader;

        public DevicePoller(ILogger<DevicePoller> logger, ModbusReader modbusReader)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _modbusReader = modbusReader ?? throw new ArgumentNullException(nameof(modbusReader));
        }
    }
}
