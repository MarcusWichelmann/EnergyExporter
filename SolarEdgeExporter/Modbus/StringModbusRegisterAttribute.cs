namespace SolarEdgeExporter.Modbus
{
    /// <inheritdoc />
    public class StringModbusRegisterAttribute : ModbusRegisterAttribute
    {
        public int Length { get; }

        public StringModbusRegisterAttribute(ushort relativeRegister, int length) : base(relativeRegister)
        {
            Length = length;
        }
    }
}
