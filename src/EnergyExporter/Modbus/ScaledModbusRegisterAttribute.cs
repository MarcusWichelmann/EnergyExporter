using System;
using System.Collections.Generic;
using System.Linq;

namespace EnergyExporter.Modbus;

/// <summary>
/// A SunSpec style value whose real value is <c>raw * 10^scaleFactor</c>.
/// Registers holding the SunSpec "not implemented" marker (0xFFFF for uint16, 0x8000 for int16 and
/// scale factors, 0x80000000 for int32) are read as <see cref="double.NaN"/>, so they are never exported
/// as real measurements.
/// </summary>
public class ScaledModbusRegisterAttribute : ModbusRegisterAttribute
{
    public Type RegisterType { get; }
    public ushort RelativeScaleFactorRegisterAddress { get; }
    public Type ScaleFactorRegisterType { get; }

    public ScaledModbusRegisterAttribute(
        ushort relativeRegisterAddress,
        Type registerType,
        ushort relativeScaleFactorRegisterAddress,
        Type scaleFactorRegisterType,
        RegisterEndianness endianness = RegisterEndianness.BigEndian) : base(relativeRegisterAddress, endianness)
    {
        RegisterType = registerType;
        RelativeScaleFactorRegisterAddress = relativeScaleFactorRegisterAddress;
        ScaleFactorRegisterType = scaleFactorRegisterType;
    }

    public override IEnumerable<ushort> GetRelativeAddressesToRead(Type propertyType)
    {
        int registerCount = ModbusUtils.GetValueRegisterCount(RegisterType);
        int scaleFactorRegisterCount = ModbusUtils.GetValueRegisterCount(ScaleFactorRegisterType);
        return Enumerable.Range(RelativeRegisterAddress, registerCount)
            .Concat(Enumerable.Range(RelativeScaleFactorRegisterAddress, scaleFactorRegisterCount))
            .Select(i => (ushort)i);
    }

    public override object Read(ReadOnlySpan<byte> data, Type propertyType)
    {
        if (propertyType != typeof(double))
            throw new ModbusReadException("Scaled modbus register properties should have the type double.");

        object rawScaleFactor = ModbusUtils.ReadValue(
            ScaleFactorRegisterType,
            data[(RelativeScaleFactorRegisterAddress * ModbusUtils.SingleRegisterSize)..],
            Endianness);
        object rawValue = ModbusUtils.ReadValue(
            RegisterType,
            data[(RelativeRegisterAddress * ModbusUtils.SingleRegisterSize)..],
            Endianness);

        if (IsNotImplemented(rawValue) || IsNotImplementedScaleFactor(rawScaleFactor))
            return double.NaN;

        var scaleFactor = Convert.ToInt32(rawScaleFactor);
        var value = Convert.ToDouble(rawValue);

        // Dividing by a power of ten (instead of multiplying with a negative one) avoids results like 240.70000000000002.
        return scaleFactor >= 0
            ? value * Math.Pow(10, scaleFactor)
            : value / Math.Pow(10, -scaleFactor);
    }

    private static bool IsNotImplemented(object rawValue) =>
        rawValue switch {
            ushort v => v == ushort.MaxValue,
            short v => v == short.MinValue,
            int v => v == int.MinValue,
            // uint values are accumulators (acc32), where 0 means "not accumulated" and is a valid reading.
            _ => false
        };

    private static bool IsNotImplementedScaleFactor(object rawScaleFactor) =>
        rawScaleFactor switch {
            short v => v == short.MinValue,
            ushort v => v == 0x8000,
            _ => false
        };
}
