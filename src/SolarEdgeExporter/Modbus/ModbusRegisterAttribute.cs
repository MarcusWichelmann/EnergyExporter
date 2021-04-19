using System;
using System.Buffers.Binary;

namespace SolarEdgeExporter.Modbus
{
    /// <summary>
    /// Specifies where the data for a property can be found on the modbus.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ModbusRegisterAttribute : Attribute
    {
        public ushort RelativeRegisterAddress { get; }

        public ModbusRegisterAttribute(ushort relativeRegisterAddress)
        {
            RelativeRegisterAddress = relativeRegisterAddress;
        }

        public virtual object ReadValue(Span<byte> registers, Type propertyType)
        {
            return ReadValueByType(registers[(RelativeRegisterAddress * 2)..], propertyType);
        }

        protected static object ReadValueByType(Span<byte> span, Type type)
        {
            if (type == typeof(ushort))
                return BinaryPrimitives.ReadUInt16BigEndian(span);
            if (type == typeof(short))
                return BinaryPrimitives.ReadInt16BigEndian(span);
            if (type == typeof(uint))
                return BinaryPrimitives.ReadUInt32BigEndian(span);
            if (type == typeof(int))
                return BinaryPrimitives.ReadInt32BigEndian(span);
            if (type == typeof(ulong))
                return BinaryPrimitives.ReadUInt64BigEndian(span);
            if (type == typeof(long))
                return BinaryPrimitives.ReadInt64BigEndian(span);
            if (type == typeof(float))
                return BinaryPrimitives.ReadSingleBigEndian(span);
            if (type == typeof(double))
                return BinaryPrimitives.ReadDoubleBigEndian(span);
            throw new ModbusReadException($"Unsupported type: {type.Name}");
        }
    }
}
