using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace SolarEdgeExporter.Modbus; 

public enum RegisterEndianness {
    BigEndian,
    LittleEndian,
    MidLittleEndian
}

public static class ModbusUtils {
    public const ushort SingleRegisterSize = 2;

    public static int GetValueRegisterCount(Type valueType) => GetValueSize(valueType) / SingleRegisterSize;

    public static int GetValueSize(Type valueType) {
        if (valueType.IsEnum)
            valueType = valueType.GetEnumUnderlyingType();

        if (!valueType.IsPrimitive)
            throw new ArgumentException("Type is not a primitive type.", nameof(valueType));

        return Marshal.SizeOf(valueType);
    }

    public static object ReadValue(Type valueType, ReadOnlySpan<byte> data, RegisterEndianness endianness) {
        if (valueType.IsEnum)
            valueType = valueType.GetEnumUnderlyingType();

        int valueSize = GetValueSize(valueType);
        if (valueSize < 2)
            throw new ArgumentException(
                "The value type is too small. Registers are at least 2-bytes wide.",
                nameof(valueType));

        // If values are encoded as mid-little-endian, swap each 2-byte pairs
        if (endianness == RegisterEndianness.MidLittleEndian) {
            Span<byte> swapped = new byte[valueSize];
            for (var i = 0; i < valueSize; i += 2) {
                swapped[i] = data[i + 1];
                swapped[i + 1] = data[i];
            }

            data = swapped;
        }

        // Read the value as little endian
        if (valueType == typeof(ushort))
            return endianness == RegisterEndianness.BigEndian
                ? BinaryPrimitives.ReadUInt16BigEndian(data)
                : BinaryPrimitives.ReadUInt16LittleEndian(data);
        if (valueType == typeof(short))
            return endianness == RegisterEndianness.BigEndian
                ? BinaryPrimitives.ReadInt16BigEndian(data)
                : BinaryPrimitives.ReadInt16LittleEndian(data);
        if (valueType == typeof(uint))
            return endianness == RegisterEndianness.BigEndian
                ? BinaryPrimitives.ReadUInt32BigEndian(data)
                : BinaryPrimitives.ReadUInt32LittleEndian(data);
        if (valueType == typeof(int))
            return endianness == RegisterEndianness.BigEndian
                ? BinaryPrimitives.ReadInt32BigEndian(data)
                : BinaryPrimitives.ReadInt32LittleEndian(data);
        if (valueType == typeof(ulong))
            return endianness == RegisterEndianness.BigEndian
                ? BinaryPrimitives.ReadUInt64BigEndian(data)
                : BinaryPrimitives.ReadUInt64LittleEndian(data);
        if (valueType == typeof(long))
            return endianness == RegisterEndianness.BigEndian
                ? BinaryPrimitives.ReadInt64BigEndian(data)
                : BinaryPrimitives.ReadInt64LittleEndian(data);
        if (valueType == typeof(float))
            return endianness == RegisterEndianness.BigEndian
                ? BinaryPrimitives.ReadSingleBigEndian(data)
                : BinaryPrimitives.ReadSingleLittleEndian(data);
        if (valueType == typeof(double))
            return endianness == RegisterEndianness.BigEndian
                ? BinaryPrimitives.ReadDoubleBigEndian(data)
                : BinaryPrimitives.ReadDoubleLittleEndian(data);
        throw new ArgumentException("Unsupported value type.", nameof(valueType));
    }
}
