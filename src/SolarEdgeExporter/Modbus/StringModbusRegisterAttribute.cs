using System;
using System.Collections.Generic;
using System.Linq;
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

        /// <inheritdoc />
        public override IEnumerable<ushort> GetRelativeAddressesToRead(Type propertyType)
        {
            int registerSize = Length / ModbusUtils.SingleRegisterSize;
            return Enumerable.Range(RelativeRegisterAddress, registerSize).Select(i => (ushort)i);
        }

        public override object Read(ReadOnlySpan<byte> data, Type propertyType)
            => Encoding.UTF8.GetString(data.Slice(RelativeRegisterAddress * ModbusUtils.SingleRegisterSize, Length)).TrimEnd('\0', ' ');
    }
}
