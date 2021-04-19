using System;

namespace SolarEdgeExporter.Modbus
{
    public class ModbusReadException : Exception
    {
        public ModbusReadException() { }
        public ModbusReadException(string? message) : base(message) { }
        public ModbusReadException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}
