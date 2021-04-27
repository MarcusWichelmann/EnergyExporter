using SolarEdgeExporter.Modbus;

namespace SolarEdgeExporter.Devices
{
    public enum BatteryStatus : uint
    {
        Off = 0,
        Standby = 1,
        Initializing = 2,
        Charging = 3,
        Discharging = 4,
        Fault = 5,
        Idle = 7,
    }

    public class Battery : IDevice
    {
        [StringModbusRegister(0, 32)]
        public string? Manufacturer { get; init; }

        [StringModbusRegister(16, 32)]
        public string? Model { get; init; }

        [StringModbusRegister(32, 32)]
        public string? Version { get; init; }

        [StringModbusRegister(48, 32)]
        public string? SerialNumber { get; init; }

        [ModbusRegister(64, RegisterEndianness.MidLittleEndian)]
        public ushort DeviceAddress { get; init; }

        [ModbusRegister(66, RegisterEndianness.MidLittleEndian)]
        public float RatedCapacity { get; init; }

        [ModbusRegister(68, RegisterEndianness.MidLittleEndian)]
        public float MaxChargeContinuousPower { get; init; }

        [ModbusRegister(70, RegisterEndianness.MidLittleEndian)]
        public float MaxDischargeContinuousPower { get; init; }

        [ModbusRegister(72, RegisterEndianness.MidLittleEndian)]
        public float MaxChargePeakPower { get; init; }

        [ModbusRegister(74, RegisterEndianness.MidLittleEndian)]
        public float MaxDischargePeakPower { get; init; }

        [ModbusRegister(108, RegisterEndianness.MidLittleEndian)]
        public float AvgTemperature { get; init; }

        [ModbusRegister(110, RegisterEndianness.MidLittleEndian)]
        public float MaxTemperature { get; init; }

        [ModbusRegister(112, RegisterEndianness.MidLittleEndian)]
        public float Voltage { get; init; }

        [ModbusRegister(114, RegisterEndianness.MidLittleEndian)]
        public float Current { get; init; }

        [ModbusRegister(116, RegisterEndianness.MidLittleEndian)]
        public float Power { get; init; }

        [ModbusRegister(118, RegisterEndianness.MidLittleEndian)]
        public ulong LifetimeExportedEnergy { get; init; }

        [ModbusRegister(122, RegisterEndianness.MidLittleEndian)]
        public ulong LifetimeImportedEnergy { get; init; }

        [ModbusRegister(126, RegisterEndianness.MidLittleEndian)]
        public float BatteryCapacity { get; init; }

        [ModbusRegister(128, RegisterEndianness.MidLittleEndian)]
        public float BatteryCharge { get; init; }

        [ModbusRegister(130, RegisterEndianness.MidLittleEndian)]
        public float BatteryCapacityPercent { get; init; }

        [ModbusRegister(132, RegisterEndianness.MidLittleEndian)]
        public float BatteryChargePercent { get; init; }

        [ModbusRegister(134, RegisterEndianness.MidLittleEndian)]
        public BatteryStatus Status { get; init; }

        [ModbusRegister(136, RegisterEndianness.MidLittleEndian)]
        public uint VendorStatus { get; init; }

        [ModbusRegister(138, RegisterEndianness.MidLittleEndian)]
        public ushort LastEvent { get; init; }
    }
}
