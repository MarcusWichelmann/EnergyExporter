using System;

namespace SolarEdgeExporter.Modbus
{
    /// <inheritdoc />
    public class ScaledModbusRegisterAttribute : ModbusRegisterAttribute
    {
        public Type RegisterType { get; }
        public ushort RelativeScaleFactorRegisterAddress { get; }

        public ScaledModbusRegisterAttribute(ushort relativeRegisterAddress, Type registerType, ushort relativeScaleFactorRegisterAddress) :
            base(relativeRegisterAddress)
        {
            RegisterType = registerType ?? throw new ArgumentNullException(nameof(registerType));
            RelativeScaleFactorRegisterAddress = relativeScaleFactorRegisterAddress;
        }

        public override object ReadValue(Span<byte> registers, Type propertyType)
        {
            if (propertyType != typeof(double))
                throw new ModbusReadException("Scaled modbus register properties should have the type double.");

            var scaleFactor = (short) ReadRegisterOfType(registers[RelativeScaleFactorRegisterAddress..], typeof(short));
            return Convert.ToDouble(ReadRegisterOfType(registers[RelativeRegisterAddress..], RegisterType)) * Math.Pow(10, scaleFactor);
        }
    }
}