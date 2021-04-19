using System;

namespace SolarEdgeExporter.Modbus
{
    /// <inheritdoc />
    public class ScaledModbusRegisterAttribute : ModbusRegisterAttribute
    {
        public Type RegisterType { get; }
        public ushort RelativeScaleFactorRegisterAddress { get; }
        public Type ScaleFactorRegisterType { get; }

        public ScaledModbusRegisterAttribute(ushort relativeRegisterAddress, Type registerType,
            ushort relativeScaleFactorRegisterAddress, Type scaleFactorRegisterType) : base(relativeRegisterAddress)
        {
            RegisterType = registerType ?? throw new ArgumentNullException(nameof(registerType));
            RelativeScaleFactorRegisterAddress = relativeScaleFactorRegisterAddress;
            ScaleFactorRegisterType = scaleFactorRegisterType
                ?? throw new ArgumentNullException(nameof(scaleFactorRegisterType));
        }

        public override object ReadValue(Span<byte> registers, Type propertyType)
        {
            if (propertyType != typeof(double))
                throw new ModbusReadException("Scaled modbus register properties should have the type double.");

            var scaleFactor = Convert.ToInt32(ReadValueByType(registers[(RelativeScaleFactorRegisterAddress * 2)..],
                ScaleFactorRegisterType));
            return Convert.ToDouble(ReadValueByType(registers[(RelativeRegisterAddress * 2)..], RegisterType))
                * Math.Pow(10, scaleFactor);
        }
    }
}
