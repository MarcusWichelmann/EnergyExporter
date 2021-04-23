using System;
using SolarEdgeExporter.Modbus;

namespace SolarEdgeExporter.Devices
{
    public enum MeterType : ushort
    {
        SinglePhase = 201,
        SplitSinglePhase = 202,
        WyeConnectThreePhase = 203,
        DeltaConnectThreePhase = 204
    }

    [Flags]
    public enum MeterEvents : uint
    {
        PowerFailure = 0x4,
        UnderVoltage = 0x8,
        LowPowerFactor = 0x10,
        OverCurrent = 0x20,
        OverVoltage = 0x40,
        MissingSensor = 0x80,
        Reserved1 = 0x100,
        Reserved2 = 0x200,
        Reserved3 = 0x400,
        Reserved4 = 0x800,
        Reserved5 = 0x1000,
        Reserved6 = 0x2000,
        Reserved7 = 0x4000,
        Reserved8 = 0x8000
    }

    public class Meter : IDevice
    {
        [StringModbusRegister(2, 32)]
        public string? Manufacturer { get; init; }

        [StringModbusRegister(18, 32)]
        public string? Model { get; init; }

        [StringModbusRegister(34, 16)]
        public string? Option { get; init; }

        [StringModbusRegister(42, 16)]
        public string? Version { get; init; }

        [StringModbusRegister(50, 32)]
        public string? SerialNumber { get; init; }

        [ModbusRegister(66)]
        public ushort DeviceAddress { get; init; }

        [ModbusRegister(67)]
        public MeterType Type { get; init; }

        [ScaledModbusRegister(69, typeof(short), 73, typeof(short))]
        public double AcCurrent { get; init; }

        [ScaledModbusRegister(70, typeof(short), 73, typeof(short))]
        public double AcCurrentP1 { get; init; }

        [ScaledModbusRegister(71, typeof(short), 73, typeof(short))]
        public double AcCurrentP2 { get; init; }

        [ScaledModbusRegister(72, typeof(short), 73, typeof(short))]
        public double AcCurrentP3 { get; init; }

        [ScaledModbusRegister(74, typeof(short), 82, typeof(short))]
        public double AcVoltageAvg { get; init; }

        [ScaledModbusRegister(75, typeof(short), 82, typeof(short))]
        public double AcVoltageP1 { get; init; }

        [ScaledModbusRegister(76, typeof(short), 82, typeof(short))]
        public double AcVoltageP2 { get; init; }

        [ScaledModbusRegister(77, typeof(short), 82, typeof(short))]
        public double AcVoltageP3 { get; init; }

        [ScaledModbusRegister(78, typeof(short), 82, typeof(short))]
        public double AcVoltageLineToLineAvg { get; init; }

        [ScaledModbusRegister(79, typeof(short), 82, typeof(short))]
        public double AcVoltageP1ToP2 { get; init; }

        [ScaledModbusRegister(80, typeof(short), 82, typeof(short))]
        public double AcVoltageP2ToP3 { get; init; }

        [ScaledModbusRegister(81, typeof(short), 82, typeof(short))]
        public double AcVoltageP3ToP1 { get; init; }

        [ScaledModbusRegister(83, typeof(short), 84, typeof(short))]
        public double AcFrequency { get; init; }

        [ScaledModbusRegister(85, typeof(short), 89, typeof(short))]
        public double AcPower { get; init; }

        [ScaledModbusRegister(86, typeof(short), 89, typeof(short))]
        public double AcPowerP1 { get; init; }

        [ScaledModbusRegister(87, typeof(short), 89, typeof(short))]
        public double AcPowerP2 { get; init; }

        [ScaledModbusRegister(88, typeof(short), 89, typeof(short))]
        public double AcPowerP3 { get; init; }

        [ScaledModbusRegister(90, typeof(short), 94, typeof(short))]
        public double AcPowerApparent { get; init; }

        [ScaledModbusRegister(91, typeof(short), 94, typeof(short))]
        public double AcPowerApparentP1 { get; init; }

        [ScaledModbusRegister(92, typeof(short), 94, typeof(short))]
        public double AcPowerApparentP2 { get; init; }

        [ScaledModbusRegister(93, typeof(short), 94, typeof(short))]
        public double AcPowerApparentP3 { get; init; }

        [ScaledModbusRegister(95, typeof(short), 99, typeof(short))]
        public double AcPowerReactive { get; init; }

        [ScaledModbusRegister(96, typeof(short), 99, typeof(short))]
        public double AcPowerReactiveP1 { get; init; }

        [ScaledModbusRegister(97, typeof(short), 99, typeof(short))]
        public double AcPowerReactiveP2 { get; init; }

        [ScaledModbusRegister(98, typeof(short), 99, typeof(short))]
        public double AcPowerReactiveP3 { get; init; }

        [ScaledModbusRegister(100, typeof(short), 104, typeof(short))]
        public double AcPowerFactor { get; init; }

        [ScaledModbusRegister(101, typeof(short), 104, typeof(short))]
        public double AcPowerFactorP1 { get; init; }

        [ScaledModbusRegister(102, typeof(short), 104, typeof(short))]
        public double AcPowerFactorP2 { get; init; }

        [ScaledModbusRegister(103, typeof(short), 104, typeof(short))]
        public double AcPowerFactorP3 { get; init; }

        [ScaledModbusRegister(105, typeof(uint), 121, typeof(short))]
        public double ExportedEnergy { get; init; }

        [ScaledModbusRegister(107, typeof(uint), 121, typeof(short))]
        public double ExportedEnergyP1 { get; init; }

        [ScaledModbusRegister(109, typeof(uint), 121, typeof(short))]
        public double ExportedEnergyP2 { get; init; }

        [ScaledModbusRegister(111, typeof(uint), 121, typeof(short))]
        public double ExportedEnergyP3 { get; init; }

        [ScaledModbusRegister(113, typeof(uint), 121, typeof(short))]
        public double ImportedEnergy { get; init; }

        [ScaledModbusRegister(115, typeof(uint), 121, typeof(short))]
        public double ImportedEnergyP1 { get; init; }

        [ScaledModbusRegister(117, typeof(uint), 121, typeof(short))]
        public double ImportedEnergyP2 { get; init; }

        [ScaledModbusRegister(119, typeof(uint), 121, typeof(short))]
        public double ImportedEnergyP3 { get; init; }

        [ScaledModbusRegister(122, typeof(uint), 138, typeof(short))]
        public double ExportedEnergyApparent { get; init; }

        [ScaledModbusRegister(124, typeof(uint), 138, typeof(short))]
        public double ExportedEnergyApparentP1 { get; init; }

        [ScaledModbusRegister(126, typeof(uint), 138, typeof(short))]
        public double ExportedEnergyApparentP2 { get; init; }

        [ScaledModbusRegister(128, typeof(uint), 138, typeof(short))]
        public double ExportedEnergyApparentP3 { get; init; }

        [ScaledModbusRegister(130, typeof(uint), 138, typeof(short))]
        public double ImportedEnergyApparent { get; init; }

        [ScaledModbusRegister(132, typeof(uint), 138, typeof(short))]
        public double ImportedEnergyApparentP1 { get; init; }

        [ScaledModbusRegister(134, typeof(uint), 138, typeof(short))]
        public double ImportedEnergyApparentP2 { get; init; }

        [ScaledModbusRegister(136, typeof(uint), 138, typeof(short))]
        public double ImportedEnergyApparentP3 { get; init; }

        [ScaledModbusRegister(139, typeof(uint), 171, typeof(short))]
        public double ImportedEnergyReactiveQ1 { get; init; }

        [ScaledModbusRegister(141, typeof(uint), 171, typeof(short))]
        public double ImportedEnergyReactiveQ1P1 { get; init; }

        [ScaledModbusRegister(143, typeof(uint), 171, typeof(short))]
        public double ImportedEnergyReactiveQ1P2 { get; init; }

        [ScaledModbusRegister(145, typeof(uint), 171, typeof(short))]
        public double ImportedEnergyReactiveQ1P3 { get; init; }

        [ScaledModbusRegister(147, typeof(uint), 171, typeof(short))]
        public double ImportedEnergyReactiveQ2 { get; init; }

        [ScaledModbusRegister(149, typeof(uint), 171, typeof(short))]
        public double ImportedEnergyReactiveQ2P1 { get; init; }

        [ScaledModbusRegister(151, typeof(uint), 171, typeof(short))]
        public double ImportedEnergyReactiveQ2P2 { get; init; }

        [ScaledModbusRegister(153, typeof(uint), 171, typeof(short))]
        public double ImportedEnergyReactiveQ2P3 { get; init; }

        [ScaledModbusRegister(155, typeof(uint), 171, typeof(short))]
        public double ExportedEnergyReactiveQ3 { get; init; }

        [ScaledModbusRegister(157, typeof(uint), 171, typeof(short))]
        public double ExportedEnergyReactiveQ3P1 { get; init; }

        [ScaledModbusRegister(159, typeof(uint), 171, typeof(short))]
        public double ExportedEnergyReactiveQ3P2 { get; init; }

        [ScaledModbusRegister(161, typeof(uint), 171, typeof(short))]
        public double ExportedEnergyReactiveQ3P3 { get; init; }

        [ScaledModbusRegister(163, typeof(uint), 171, typeof(short))]
        public double ExportedEnergyReactiveQ4 { get; init; }

        [ScaledModbusRegister(165, typeof(uint), 171, typeof(short))]
        public double ExportedEnergyReactiveQ4P1 { get; init; }

        [ScaledModbusRegister(167, typeof(uint), 171, typeof(short))]
        public double ExportedEnergyReactiveQ4P2 { get; init; }

        [ScaledModbusRegister(169, typeof(uint), 171, typeof(short))]
        public double ExportedEnergyReactiveQ4P3 { get; init; }

        [ModbusRegister(172)]
        public MeterEvents Events { get; init; }
    }
}
