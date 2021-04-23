using SolarEdgeExporter.Modbus;
using SolarEdgeExporter.Prometheus;

namespace SolarEdgeExporter.Devices
{
    public enum InverterType : ushort
    {
        SinglePhase = 101,
        SplitPhase = 102,
        ThreePhase = 103
    }

    public enum InverterStatus : ushort
    {
        Off = 1,
        Sleeping = 2,
        Starting = 3,
        Producing = 4,
        Throttled = 5,
        ShuttingDown = 6,
        Fault = 7,
        Standby = 8
    }

    public class Inverter : IDevice
    {
        [StringModbusRegister(4, 32)]
        public string? Manufacturer { get; init; }

        [StringModbusRegister(20, 32)]
        public string? Model { get; init; }

        [StringModbusRegister(44, 16)]
        public string? Version { get; init; }

        [StringModbusRegister(52, 32)]
        public string? SerialNumber { get; init; }

        [ModbusRegister(68)]
        public ushort DeviceAddress { get; init; }

        [ModbusRegister(69)]
        public InverterType Type { get; init; }

        [ScaledModbusRegister(71, typeof(ushort), 75, typeof(short))]
        public double AcCurrent { get; init; }

        [ScaledModbusRegister(72, typeof(ushort), 75, typeof(short))]
        public double AcCurrentP1 { get; init; }

        [ScaledModbusRegister(73, typeof(ushort), 75, typeof(short))]
        public double AcCurrentP2 { get; init; }

        [ScaledModbusRegister(74, typeof(ushort), 75, typeof(short))]
        public double AcCurrentP3 { get; init; }

        [ScaledModbusRegister(76, typeof(ushort), 82, typeof(short))]
        public double AcVoltageP1ToP2 { get; init; }

        [ScaledModbusRegister(77, typeof(ushort), 82, typeof(short))]
        public double AcVoltageP2ToP3 { get; init; }

        [ScaledModbusRegister(78, typeof(ushort), 82, typeof(short))]
        public double AcVoltageP3ToP1 { get; init; }

        [ScaledModbusRegister(79, typeof(ushort), 82, typeof(short))]
        public double AcVoltageP1 { get; init; }

        [ScaledModbusRegister(80, typeof(ushort), 82, typeof(short))]
        public double AcVoltageP2 { get; init; }

        [ScaledModbusRegister(81, typeof(ushort), 82, typeof(short))]
        public double AcVoltageP3 { get; init; }

        [ScaledModbusRegister(83, typeof(short), 84, typeof(short))]
        public double AcPower { get; init; }

        [ScaledModbusRegister(85, typeof(ushort), 86, typeof(short))]
        public double AcFrequency { get; init; }

        [ScaledModbusRegister(87, typeof(short), 88, typeof(short))]
        public double AcPowerApparent { get; init; }

        [ScaledModbusRegister(89, typeof(short), 90, typeof(short))]
        public double AcPowerReactive { get; init; }

        [ScaledModbusRegister(91, typeof(short), 92, typeof(short))]
        public double AcPowerFactor { get; init; }

        [ScaledModbusRegister(93, typeof(uint), 95, typeof(ushort))]
        public double AcTotalEnergyProduction { get; init; }

        [ScaledModbusRegister(96, typeof(ushort), 97, typeof(short))]
        public double DcCurrent { get; init; }

        [ScaledModbusRegister(98, typeof(ushort), 99, typeof(short))]
        public double DcVoltage { get; init; }

        [ScaledModbusRegister(100, typeof(short), 101, typeof(short))]
        public double DcPower { get; init; }

        [ScaledModbusRegister(103, typeof(short), 106, typeof(short))]
        public double HeatSinkTemperature { get; init; }

        [ModbusRegister(107)]
        public InverterStatus Status { get; init; }

        [ModbusRegister(108)]
        public ushort VendorStatus { get; init; }
    }
}
