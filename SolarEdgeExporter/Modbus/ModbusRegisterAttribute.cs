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
            return ReadRegisterOfType(registers[RelativeRegisterAddress..], propertyType);
        }

        protected static object ReadRegisterOfType(Span<byte> registerSpan, Type type)
        {
            if (type == typeof(ushort))
                return BinaryPrimitives.ReadUInt16BigEndian(registerSpan);
            if (type == typeof(short))
                return BinaryPrimitives.ReadInt16BigEndian(registerSpan);
            if (type == typeof(uint))
                return BinaryPrimitives.ReadUInt32BigEndian(registerSpan);
            if (type == typeof(int))
                return BinaryPrimitives.ReadInt32BigEndian(registerSpan);
            if (type == typeof(ulong))
                return BinaryPrimitives.ReadUInt64BigEndian(registerSpan);
            if (type == typeof(long))
                return BinaryPrimitives.ReadInt64BigEndian(registerSpan);
            if (type == typeof(float))
                return BinaryPrimitives.ReadSingleBigEndian(registerSpan);
            if (type == typeof(double))
                return BinaryPrimitives.ReadDoubleBigEndian(registerSpan);
            throw new ModbusReadException($"Unsupported register type: {type.Name}");
        }
    }
}