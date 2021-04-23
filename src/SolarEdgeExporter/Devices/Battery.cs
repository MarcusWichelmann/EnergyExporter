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

        [ModbusRegister(64)]
        public ushort DeviceAddress { get; init; }

        [ModbusRegister(66)]
        public float RatedCapacity { get; init; }

        [ModbusRegister(68)]
        public float MaxChargeContinuousPower { get; init; }

        [ModbusRegister(70)]
        public float MaxDischargeContinuousPower { get; init; }

        [ModbusRegister(72)]
        public float MaxChargePeakPower { get; init; }

        [ModbusRegister(74)]
        public float MaxDischargePeakPower { get; init; }

        [ModbusRegister(108)]
        public float AvgTemperature { get; init; }

        [ModbusRegister(110)]
        public float MaxTemperature { get; init; }

        [ModbusRegister(112)]
        public float Voltage { get; init; }

        [ModbusRegister(114)]
        public float Current { get; init; }

        [ModbusRegister(116)]
        public float Power { get; init; }

        [ModbusRegister(118)]
        public ulong TotalExportedEnergy { get; init; }

        [ModbusRegister(122)]
        public ulong TotalImportedEnergy { get; init; }

        [ModbusRegister(126)]
        public float BatteryCapacity { get; init; }

        [ModbusRegister(128)]
        public float BatteryCharge { get; init; }

        [ModbusRegister(130)]
        public float BatteryCapacityPercent { get; init; }

        [ModbusRegister(132)]
        public float BatteryChargePercent { get; init; }

        [ModbusRegister(134)]
        public BatteryStatus Status { get; init; }

        [ModbusRegister(136)]
        public uint VendorStatus { get; init; }

        [ModbusRegister(138)]
        public ushort LastEvent { get; init; }
    }
}
