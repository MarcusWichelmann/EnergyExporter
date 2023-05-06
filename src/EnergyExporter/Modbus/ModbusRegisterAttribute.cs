using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace EnergyExporter.Modbus;

/// <summary>
/// Specifies where the data for a property can be found on the modbus.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ModbusRegisterAttribute : Attribute
{
    public ushort RelativeRegisterAddress { get; }

    public RegisterEndianness Endianness { get; }

    public ModbusRegisterAttribute(
        ushort relativeRegisterAddress,
        RegisterEndianness endianness = RegisterEndianness.BigEndian)
    {
        if (!Enum.IsDefined(typeof(RegisterEndianness), endianness))
            throw new InvalidEnumArgumentException(nameof(endianness), (int)endianness, typeof(RegisterEndianness));

        RelativeRegisterAddress = relativeRegisterAddress;
        Endianness = endianness;
    }

    public virtual IEnumerable<ushort> GetRelativeAddressesToRead(Type propertyType)
    {
        int registerCount = ModbusUtils.GetValueRegisterCount(propertyType);
        return Enumerable.Range(RelativeRegisterAddress, registerCount).Select(i => (ushort)i);
    }

    public virtual object Read(ReadOnlySpan<byte> data, Type propertyType) =>
        ModbusUtils.ReadValue(
            propertyType,
            data[(RelativeRegisterAddress * ModbusUtils.SingleRegisterSize)..],
            Endianness);
}
