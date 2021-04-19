using System;
using System.Text;

namespace SolarEdgeExporter.Modbus
{
    /// <inheritdoc />
    public class StringModbusRegisterAttribute : ModbusRegisterAttribute
    {
        public int Length { get; }

        public StringModbusRegisterAttribute(ushort relativeRegisterAddress, int length) : base(relativeRegisterAddress)
        {
            Length = length;
        }

        public override object ReadValue(Span<byte> registers, Type propertyType)
        {
            return Encoding.UTF8.GetString(registers.Slice(RelativeRegisterAddress * 2, Length)).TrimEnd('\0', ' ');
        }
    }
}