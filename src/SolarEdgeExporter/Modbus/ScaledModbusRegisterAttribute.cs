using System;
using System.Collections.Generic;
using System.Linq;

namespace SolarEdgeExporter.Modbus
{
    /// <inheritdoc />
    public class ScaledModbusRegisterAttribute : ModbusRegisterAttribute
    {
        public Type RegisterType { get; }
        public ushort RelativeScaleFactorRegisterAddress { get; }
        public Type ScaleFactorRegisterType { get; }

        public ScaledModbusRegisterAttribute(ushort relativeRegisterAddress, Type registerType, ushort relativeScaleFactorRegisterAddress, Type scaleFactorRegisterType,
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
            return Enumerable.Range(RelativeRegisterAddress, registerCount).Concat(Enumerable.Range(RelativeScaleFactorRegisterAddress, scaleFactorRegisterCount))
                .Select(i => (ushort)i);
        }

        public override object Read(ReadOnlySpan<byte> data, Type propertyType)
        {
            if (propertyType != typeof(double))
                throw new ModbusReadException("Scaled modbus register properties should have the type double.");

            var scaleFactor = Convert.ToInt32(ModbusUtils.ReadValue(ScaleFactorRegisterType, data[(RelativeScaleFactorRegisterAddress * ModbusUtils.SingleRegisterSize)..],
                Endianness));
            var value = Convert.ToDouble(ModbusUtils.ReadValue(RegisterType, data[(RelativeRegisterAddress * ModbusUtils.SingleRegisterSize)..], Endianness));
            return value * Math.Pow(10, scaleFactor);
        }
    }
}
