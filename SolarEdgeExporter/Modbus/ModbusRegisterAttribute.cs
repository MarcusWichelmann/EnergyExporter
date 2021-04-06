using System;

namespace SolarEdgeExporter.Modbus
{
    /// <summary>
    /// Specifies where the data for a property can be found on the modbus.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ModbusRegisterAttribute : Attribute
    {
        public ushort RelativeRegister { get; }

        public ModbusRegisterAttribute(ushort relativeRegister)
        {
            RelativeRegister = relativeRegister;
        }
    }
}
