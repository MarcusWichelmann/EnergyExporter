using System;
using SolarEdgeExporter.Modbus;
using SolarEdgeExporter.Prometheus;

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
        [PrometheusMetric(MetricType.Gauge, "solaredge_meter_ac_current", "AC current")]
        public double AcCurrent { get; init; }

        [ScaledModbusRegister(70, typeof(short), 73, typeof(short))]
        [PrometheusMetric(MetricType.Gauge, "solaredge_meter_ac_current_p1", "AC current on phase 1")]
        public double AcCurrentP1 { get; init; }

        [ScaledModbusRegister(71, typeof(short), 73, typeof(short))]
        [PrometheusMetric(MetricType.Gauge, "solaredge_meter_ac_current_p2", "AC current on phase 2")]
        public double AcCurrentP2 { get; init; }

        [ScaledModbusRegister(72, typeof(short), 73, typeof(short))]
        [PrometheusMetric(MetricType.Gauge, "solaredge_meter_ac_current_p3", "AC current on phase 3")]
        public double AcCurrentP3 { get; init; }

        [ScaledModbusRegister(74, typeof(short), 82, typeof(short))]
        [PrometheusMetric(MetricType.Gauge, "solaredge_meter_ac_voltage_avg", "AC voltage average")]
        public double AcVoltageAvg { get; init; }

        [ScaledModbusRegister(75, typeof(short), 82, typeof(short))]
        [PrometheusMetric(MetricType.Gauge, "solaredge_meter_ac_voltage_p1", "AC voltage on phase 1")]
        public double AcVoltageP1 { get; init; }

        [ScaledModbusRegister(76, typeof(short), 82, typeof(short))]
        [PrometheusMetric(MetricType.Gauge, "solaredge_meter_ac_voltage_p2", "AC voltage on phase 2")]
        public double AcVoltageP2 { get; init; }

        [ScaledModbusRegister(77, typeof(short), 82, typeof(short))]
        [PrometheusMetric(MetricType.Gauge, "solaredge_meter_ac_voltage_p3", "AC voltage on phase 3")]
        public double AcVoltageP3 { get; init; }

        [ScaledModbusRegister(78, typeof(short), 82, typeof(short))]
        [PrometheusMetric(MetricType.Gauge, "solaredge_meter_ac_voltage_line_to_line_avg",
            "AC voltage line-to-line average")]
        public double AcVoltageLineToLineAvg { get; init; }

        [ScaledModbusRegister(79, typeof(short), 82, typeof(short))]
        [PrometheusMetric(MetricType.Gauge, "solaredge_meter_ac_voltage_p1_to_p2", "AC voltage between phase 1 and 2")]
        public double AcVoltageP1ToP2 { get; init; }

        [ScaledModbusRegister(80, typeof(short), 82, typeof(short))]
        [PrometheusMetric(MetricType.Gauge, "solaredge_meter_ac_voltage_p2_to_p3", "AC voltage between phase 2 and 3")]
        public double AcVoltageP2ToP3 { get; init; }

        [ScaledModbusRegister(81, typeof(short), 82, typeof(short))]
        [PrometheusMetric(MetricType.Gauge, "solaredge_meter_ac_voltage_p3_to_p1", "AC voltage between phase 3 and 1")]
        public double AcVoltageP3ToP1 { get; init; }

        [ScaledModbusRegister(83, typeof(short), 84, typeof(short))]
        [PrometheusMetric(MetricType.Gauge, "solaredge_meter_ac_frequency", "AC frequency")]
        public double AcFrequency { get; init; }

        [ScaledModbusRegister(85, typeof(short), 89, typeof(short))]
        [PrometheusMetric(MetricType.Gauge, "solaredge_meter_ac_power", "AC power")]
        public double AcPower { get; init; }

        [ScaledModbusRegister(86, typeof(short), 89, typeof(short))]
        [PrometheusMetric(MetricType.Gauge, "solaredge_meter_ac_power_p1", "AC power on phase 1")]
        public double AcPowerP1 { get; init; }

        [ScaledModbusRegister(87, typeof(short), 89, typeof(short))]
        [PrometheusMetric(MetricType.Gauge, "solaredge_meter_ac_power_p2", "AC power on phase 2")]
        public double AcPowerP2 { get; init; }

        [ScaledModbusRegister(88, typeof(short), 89, typeof(short))]
        [PrometheusMetric(MetricType.Gauge, "solaredge_meter_ac_power_p3", "AC power on phase 3")]
        public double AcPowerP3 { get; init; }

        [ScaledModbusRegister(90, typeof(short), 94, typeof(short))]
        [PrometheusMetric(MetricType.Gauge, "solaredge_meter_ac_power_apparent", "AC power apparent")]
        public double AcPowerApparent { get; init; }

        [ScaledModbusRegister(91, typeof(short), 94, typeof(short))]
        [PrometheusMetric(MetricType.Gauge, "solaredge_meter_ac_power_apparent_p1", "AC power apparent on phase 1")]
        public double AcPowerApparentP1 { get; init; }

        [ScaledModbusRegister(92, typeof(short), 94, typeof(short))]
        [PrometheusMetric(MetricType.Gauge, "solaredge_meter_ac_power_apparent_p2", "AC power apparent on phase 2")]
        public double AcPowerApparentP2 { get; init; }

        [ScaledModbusRegister(93, typeof(short), 94, typeof(short))]
        [PrometheusMetric(MetricType.Gauge, "solaredge_meter_ac_power_apparent_p3", "AC power apparent on phase 3")]
        public double AcPowerApparentP3 { get; init; }

        [ScaledModbusRegister(95, typeof(short), 99, typeof(short))]
        [PrometheusMetric(MetricType.Gauge, "solaredge_meter_ac_power_reactive", "AC power reactive")]
        public double AcPowerReactive { get; init; }

        [ScaledModbusRegister(96, typeof(short), 99, typeof(short))]
        [PrometheusMetric(MetricType.Gauge, "solaredge_meter_ac_power_reactive_p1", "AC power reactive on phase 1")]
        public double AcPowerReactiveP1 { get; init; }

        [ScaledModbusRegister(97, typeof(short), 99, typeof(short))]
        [PrometheusMetric(MetricType.Gauge, "solaredge_meter_ac_power_reactive_p2", "AC power reactive on phase 2")]
        public double AcPowerReactiveP2 { get; init; }

        [ScaledModbusRegister(98, typeof(short), 99, typeof(short))]
        [PrometheusMetric(MetricType.Gauge, "solaredge_meter_ac_power_reactive_p3", "AC power reactive on phase 3")]
        public double AcPowerReactiveP3 { get; init; }

        [ScaledModbusRegister(100, typeof(short), 104, typeof(short))]
        [PrometheusMetric(MetricType.Gauge, "solaredge_meter_ac_power_factor", "AC power factor")]
        public double AcPowerFactor { get; init; }

        [ScaledModbusRegister(101, typeof(short), 104, typeof(short))]
        [PrometheusMetric(MetricType.Gauge, "solaredge_meter_ac_power_factor_p1", "AC power factor on phase 1")]
        public double AcPowerFactorP1 { get; init; }

        [ScaledModbusRegister(102, typeof(short), 104, typeof(short))]
        [PrometheusMetric(MetricType.Gauge, "solaredge_meter_ac_power_factor_p2", "AC power factor on phase 2")]
        public double AcPowerFactorP2 { get; init; }

        [ScaledModbusRegister(103, typeof(short), 104, typeof(short))]
        [PrometheusMetric(MetricType.Gauge, "solaredge_meter_ac_power_factor_p3", "AC power factor on phase 3")]
        public double AcPowerFactorP3 { get; init; }

        [ScaledModbusRegister(105, typeof(uint), 121, typeof(short))]
        [PrometheusMetric(MetricType.Counter, "solaredge_meter_exported_energy", "Exported energy")]
        public double ExportedEnergy { get; init; }

        [ScaledModbusRegister(107, typeof(uint), 121, typeof(short))]
        [PrometheusMetric(MetricType.Counter, "solaredge_meter_exported_energy_p1", "Exported energy on phase 1")]
        public double ExportedEnergyP1 { get; init; }

        [ScaledModbusRegister(109, typeof(uint), 121, typeof(short))]
        [PrometheusMetric(MetricType.Counter, "solaredge_meter_exported_energy_p2", "Exported energy on phase 2")]
        public double ExportedEnergyP2 { get; init; }

        [ScaledModbusRegister(111, typeof(uint), 121, typeof(short))]
        [PrometheusMetric(MetricType.Counter, "solaredge_meter_exported_energy_p3", "Exported energy on phase 3")]
        public double ExportedEnergyP3 { get; init; }

        [ScaledModbusRegister(113, typeof(uint), 121, typeof(short))]
        [PrometheusMetric(MetricType.Counter, "solaredge_meter_imported_energy", "Imported energy")]
        public double ImportedEnergy { get; init; }

        [ScaledModbusRegister(115, typeof(uint), 121, typeof(short))]
        [PrometheusMetric(MetricType.Counter, "solaredge_meter_imported_energy_p1", "Imported energy on phase 1")]
        public double ImportedEnergyP1 { get; init; }

        [ScaledModbusRegister(117, typeof(uint), 121, typeof(short))]
        [PrometheusMetric(MetricType.Counter, "solaredge_meter_imported_energy_p2", "Imported energy on phase 2")]
        public double ImportedEnergyP2 { get; init; }

        [ScaledModbusRegister(119, typeof(uint), 121, typeof(short))]
        [PrometheusMetric(MetricType.Counter, "solaredge_meter_imported_energy_p3", "Imported energy on phase 3")]
        public double ImportedEnergyP3 { get; init; }

        [ScaledModbusRegister(122, typeof(uint), 138, typeof(short))]
        [PrometheusMetric(MetricType.Counter, "solaredge_meter_exported_energy_apparent", "Exported energy apparent")]
        public double ExportedEnergyApparent { get; init; }

        [ScaledModbusRegister(124, typeof(uint), 138, typeof(short))]
        [PrometheusMetric(MetricType.Counter, "solaredge_meter_exported_energy_apparent_p1",
            "Exported energy apparent on phase 1")]
        public double ExportedEnergyApparentP1 { get; init; }

        [ScaledModbusRegister(126, typeof(uint), 138, typeof(short))]
        [PrometheusMetric(MetricType.Counter, "solaredge_meter_exported_energy_apparent_p2",
            "Exported energy apparent on phase 2")]
        public double ExportedEnergyApparentP2 { get; init; }

        [ScaledModbusRegister(128, typeof(uint), 138, typeof(short))]
        [PrometheusMetric(MetricType.Counter, "solaredge_meter_exported_energy_apparent_p3",
            "Exported energy apparent on phase 3")]
        public double ExportedEnergyApparentP3 { get; init; }

        [ScaledModbusRegister(130, typeof(uint), 138, typeof(short))]
        [PrometheusMetric(MetricType.Counter, "solaredge_meter_imported_energy_apparent", "Imported energy apparent")]
        public double ImportedEnergyApparent { get; init; }

        [ScaledModbusRegister(132, typeof(uint), 138, typeof(short))]
        [PrometheusMetric(MetricType.Counter, "solaredge_meter_imported_energy_apparent_p1",
            "Imported energy apparent on phase 1")]
        public double ImportedEnergyApparentP1 { get; init; }

        [ScaledModbusRegister(134, typeof(uint), 138, typeof(short))]
        [PrometheusMetric(MetricType.Counter, "solaredge_meter_imported_energy_apparent_p2",
            "Imported energy apparent on phase 2")]
        public double ImportedEnergyApparentP2 { get; init; }

        [ScaledModbusRegister(136, typeof(uint), 138, typeof(short))]
        [PrometheusMetric(MetricType.Counter, "solaredge_meter_imported_energy_apparent_p3",
            "Imported energy apparent on phase 3")]
        public double ImportedEnergyApparentP3 { get; init; }

        [ScaledModbusRegister(139, typeof(uint), 171, typeof(short))]
        [PrometheusMetric(MetricType.Counter, "solaredge_meter_imported_energy_reactive_q1",
            "Imported energy reactive Q1")]
        public double ImportedEnergyReactiveQ1 { get; init; }

        [ScaledModbusRegister(141, typeof(uint), 171, typeof(short))]
        [PrometheusMetric(MetricType.Counter, "solaredge_meter_imported_energy_reactive_q1_p1",
            "Imported energy reactive Q1 on phase 1")]
        public double ImportedEnergyReactiveQ1P1 { get; init; }

        [ScaledModbusRegister(143, typeof(uint), 171, typeof(short))]
        [PrometheusMetric(MetricType.Counter, "solaredge_meter_imported_energy_reactive_q1_p2",
            "Imported energy reactive Q1 on phase 2")]
        public double ImportedEnergyReactiveQ1P2 { get; init; }

        [ScaledModbusRegister(145, typeof(uint), 171, typeof(short))]
        [PrometheusMetric(MetricType.Counter, "solaredge_meter_imported_energy_reactive_q1_p3",
            "Imported energy reactive Q1 on phase 3")]
        public double ImportedEnergyReactiveQ1P3 { get; init; }

        [ScaledModbusRegister(147, typeof(uint), 171, typeof(short))]
        [PrometheusMetric(MetricType.Counter, "solaredge_meter_imported_energy_reactive_q2",
            "Imported energy reactive Q2")]
        public double ImportedEnergyReactiveQ2 { get; init; }

        [ScaledModbusRegister(149, typeof(uint), 171, typeof(short))]
        [PrometheusMetric(MetricType.Counter, "solaredge_meter_imported_energy_reactive_q2_p1",
            "Imported energy reactive Q2 on phase 1")]
        public double ImportedEnergyReactiveQ2P1 { get; init; }

        [ScaledModbusRegister(151, typeof(uint), 171, typeof(short))]
        [PrometheusMetric(MetricType.Counter, "solaredge_meter_imported_energy_reactive_q2_p2",
            "Imported energy reactive Q2 on phase 2")]
        public double ImportedEnergyReactiveQ2P2 { get; init; }

        [ScaledModbusRegister(153, typeof(uint), 171, typeof(short))]
        [PrometheusMetric(MetricType.Counter, "solaredge_meter_imported_energy_reactive_q2_p3",
            "Imported energy reactive Q2 on phase 3")]
        public double ImportedEnergyReactiveQ2P3 { get; init; }

        [ScaledModbusRegister(155, typeof(uint), 171, typeof(short))]
        [PrometheusMetric(MetricType.Counter, "solaredge_meter_exported_energy_reactive_q3",
            "Exported energy reactive Q3")]
        public double ExportedEnergyReactiveQ3 { get; init; }

        [ScaledModbusRegister(157, typeof(uint), 171, typeof(short))]
        [PrometheusMetric(MetricType.Counter, "solaredge_meter_exported_energy_reactive_q3_p1",
            "Exported energy reactive Q3 on phase 1")]
        public double ExportedEnergyReactiveQ3P1 { get; init; }

        [ScaledModbusRegister(159, typeof(uint), 171, typeof(short))]
        [PrometheusMetric(MetricType.Counter, "solaredge_meter_exported_energy_reactive_q3_p2",
            "Exported energy reactive Q3 on phase 2")]
        public double ExportedEnergyReactiveQ3P2 { get; init; }

        [ScaledModbusRegister(161, typeof(uint), 171, typeof(short))]
        [PrometheusMetric(MetricType.Counter, "solaredge_meter_exported_energy_reactive_q3_p3",
            "Exported energy reactive Q3 on phase 3")]
        public double ExportedEnergyReactiveQ3P3 { get; init; }

        [ScaledModbusRegister(163, typeof(uint), 171, typeof(short))]
        [PrometheusMetric(MetricType.Counter, "solaredge_meter_exported_energy_reactive_q4",
            "Exported energy reactive Q4")]
        public double ExportedEnergyReactiveQ4 { get; init; }

        [ScaledModbusRegister(165, typeof(uint), 171, typeof(short))]
        [PrometheusMetric(MetricType.Counter, "solaredge_meter_exported_energy_reactive_q4_p1",
            "Exported energy reactive Q4 on phase 1")]
        public double ExportedEnergyReactiveQ4P1 { get; init; }

        [ScaledModbusRegister(167, typeof(uint), 171, typeof(short))]
        [PrometheusMetric(MetricType.Counter, "solaredge_meter_exported_energy_reactive_q4_p2",
            "Exported energy reactive Q4 on phase 2")]
        public double ExportedEnergyReactiveQ4P2 { get; init; }

        [ScaledModbusRegister(169, typeof(uint), 171, typeof(short))]
        [PrometheusMetric(MetricType.Counter, "solaredge_meter_exported_energy_reactive_q4_p3",
            "Exported energy reactive Q4 on phase 3")]
        public double ExportedEnergyReactiveQ4P3 { get; init; }

        [ModbusRegister(172)]
        public MeterEvents Events { get; init; }
    }
}
