using System;

namespace SolarEdgeExporter.Modbus
{
    /// <inheritdoc />
    public class ScaledModbusRegisterAttribute : ModbusRegisterAttribute
    {
        public Type OriginalType { get; }
        public ushort ScaleFactorAddress { get; }

        public ScaledModbusRegisterAttribute(ushort relativeRegister, Type originalType, ushort scaleFactorAddress) :
            base(relativeRegister)
        {
            OriginalType = originalType ?? throw new ArgumentNullException(nameof(originalType));
            ScaleFactorAddress = scaleFactorAddress;
        }
    }
}
