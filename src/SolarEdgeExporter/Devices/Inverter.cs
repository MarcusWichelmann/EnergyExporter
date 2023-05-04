using System.Text.Json.Serialization;
using SolarEdgeExporter.InfluxDb;
using SolarEdgeExporter.Modbus;
using SolarEdgeExporter.Prometheus;

namespace SolarEdgeExporter.Devices; 

public enum InverterType : ushort {
    SinglePhase = 101,
    SplitPhase = 102,
    ThreePhase = 103
}

public enum InverterStatus : ushort {
    Off = 1,
    Sleeping = 2,
    Starting = 3,
    Producing = 4,
    Throttled = 5,
    ShuttingDown = 6,
    Fault = 7,
    Standby = 8
}

public class Inverter : IDevice {
    /// <inheritdoc />
    [JsonIgnore]
    public string DeviceIdentifier => SerialNumber!;

    [StringModbusRegister(4, 32)]
    [InfluxDbMetric("manufacturer")]
    public string? Manufacturer { get; init; }

    [StringModbusRegister(20, 32)]
    [InfluxDbMetric("model")]
    public string? Model { get; init; }

    [StringModbusRegister(44, 16)]
    [InfluxDbMetric("version")]
    public string? Version { get; init; }

    [StringModbusRegister(52, 32)]
    [InfluxDbMetric("serial_number")]
    public string? SerialNumber { get; init; }

    [ModbusRegister(68)]
    [InfluxDbMetric("device_address")]
    public ushort DeviceAddress { get; init; }

    [ModbusRegister(69)]
    [InfluxDbMetric("type")]
    public InverterType Type { get; init; }

    [ScaledModbusRegister(71, typeof(ushort), 75, typeof(short))]
    [PrometheusMetric(MetricType.Gauge, "solaredge_inverter_ac_current", "AC current")]
    [InfluxDbMetric("ac_current")]
    public double AcCurrent { get; init; }

    [ScaledModbusRegister(72, typeof(ushort), 75, typeof(short))]
    [PrometheusMetric(MetricType.Gauge, "solaredge_inverter_ac_current_p1", "AC current on phase 1")]
    [InfluxDbMetric("ac_current_p1")]
    public double AcCurrentP1 { get; init; }

    [ScaledModbusRegister(73, typeof(ushort), 75, typeof(short))]
    [PrometheusMetric(MetricType.Gauge, "solaredge_inverter_ac_current_p2", "AC current on phase 2")]
    [InfluxDbMetric("ac_current_p2")]
    public double AcCurrentP2 { get; init; }

    [ScaledModbusRegister(74, typeof(ushort), 75, typeof(short))]
    [PrometheusMetric(MetricType.Gauge, "solaredge_inverter_ac_current_p3", "AC current on phase 3")]
    [InfluxDbMetric("ac_current_p3")]
    public double AcCurrentP3 { get; init; }

    [ScaledModbusRegister(76, typeof(ushort), 82, typeof(short))]
    [PrometheusMetric(MetricType.Gauge, "solaredge_inverter_ac_voltage_p1_to_p2", "AC voltage between phase 1 and 2")]
    [InfluxDbMetric("ac_voltage_p1_to_p2")]
    public double AcVoltageP1ToP2 { get; init; }

    [ScaledModbusRegister(77, typeof(ushort), 82, typeof(short))]
    [PrometheusMetric(MetricType.Gauge, "solaredge_inverter_ac_voltage_p2_to_p3", "AC voltage between phase 2 and 3")]
    [InfluxDbMetric("ac_voltage_p2_to_p3")]
    public double AcVoltageP2ToP3 { get; init; }

    [ScaledModbusRegister(78, typeof(ushort), 82, typeof(short))]
    [PrometheusMetric(MetricType.Gauge, "solaredge_inverter_ac_voltage_p3_to_p1", "AC voltage between phase 3 and 1")]
    [InfluxDbMetric("ac_voltage_p3_to_p1")]
    public double AcVoltageP3ToP1 { get; init; }

    [ScaledModbusRegister(79, typeof(ushort), 82, typeof(short))]
    [PrometheusMetric(MetricType.Gauge, "solaredge_inverter_ac_voltage_p1", "AC voltage between phase 1 and N")]
    [InfluxDbMetric("ac_voltage_p1")]
    public double AcVoltageP1 { get; init; }

    [ScaledModbusRegister(80, typeof(ushort), 82, typeof(short))]
    [PrometheusMetric(MetricType.Gauge, "solaredge_inverter_ac_voltage_p2", "AC voltage between phase 2 and N")]
    [InfluxDbMetric("ac_voltage_p2")]
    public double AcVoltageP2 { get; init; }

    [ScaledModbusRegister(81, typeof(ushort), 82, typeof(short))]
    [PrometheusMetric(MetricType.Gauge, "solaredge_inverter_ac_voltage_p3", "AC voltage between phase 3 and N")]
    [InfluxDbMetric("ac_voltage_p3")]
    public double AcVoltageP3 { get; init; }

    [ScaledModbusRegister(83, typeof(short), 84, typeof(short))]
    [PrometheusMetric(MetricType.Gauge, "solaredge_inverter_ac_power", "AC power")]
    [InfluxDbMetric("ac_power")]
    public double AcPower { get; init; }

    [ScaledModbusRegister(85, typeof(ushort), 86, typeof(short))]
    [PrometheusMetric(MetricType.Gauge, "solaredge_inverter_ac_frequency", "AC frequency")]
    [InfluxDbMetric("ac_frequency")]
    public double AcFrequency { get; init; }

    [ScaledModbusRegister(87, typeof(short), 88, typeof(short))]
    [PrometheusMetric(MetricType.Gauge, "solaredge_inverter_ac_power_apparent", "AC power apparent")]
    [InfluxDbMetric("ac_power_apparent")]
    public double AcPowerApparent { get; init; }

    [ScaledModbusRegister(89, typeof(short), 90, typeof(short))]
    [PrometheusMetric(MetricType.Gauge, "solaredge_inverter_ac_power_reactive", "AC power reactive")]
    [InfluxDbMetric("ac_power_reactive")]
    public double AcPowerReactive { get; init; }

    [ScaledModbusRegister(91, typeof(short), 92, typeof(short))]
    [PrometheusMetric(MetricType.Gauge, "solaredge_inverter_ac_power_factor", "AC power factor")]
    [InfluxDbMetric("ac_power_factor")]
    public double AcPowerFactor { get; init; }

    [ScaledModbusRegister(93, typeof(uint), 95, typeof(ushort))]
    [PrometheusMetric(MetricType.Counter, "solaredge_inverter_ac_total_energy_produced", "AC total energy produced")]
    [InfluxDbMetric("ac_total_energy_produced")]
    public double AcTotalEnergyProduced { get; init; }

    [ScaledModbusRegister(96, typeof(ushort), 97, typeof(short))]
    [PrometheusMetric(MetricType.Gauge, "solaredge_inverter_dc_current", "DC current")]
    [InfluxDbMetric("dc_current")]
    public double DcCurrent { get; init; }

    [ScaledModbusRegister(98, typeof(ushort), 99, typeof(short))]
    [PrometheusMetric(MetricType.Gauge, "solaredge_inverter_dc_voltage", "DC voltage")]
    [InfluxDbMetric("dc_voltage")]
    public double DcVoltage { get; init; }

    [ScaledModbusRegister(100, typeof(short), 101, typeof(short))]
    [PrometheusMetric(MetricType.Gauge, "solaredge_inverter_dc_power", "DC power")]
    [InfluxDbMetric("dc_power")]
    public double DcPower { get; init; }

    [ScaledModbusRegister(103, typeof(short), 106, typeof(short))]
    [PrometheusMetric(MetricType.Gauge, "solaredge_inverter_heat_sink_temperature", "Heat sink temperature")]
    [InfluxDbMetric("heat_sink_temperature")]
    public double HeatSinkTemperature { get; init; }

    [ModbusRegister(107)]
    [PrometheusMetric(MetricType.Gauge, "solaredge_inverter_status", "Status")]
    [InfluxDbMetric("status")]
    public InverterStatus Status { get; init; }

    [ModbusRegister(108)]
    [PrometheusMetric(MetricType.Gauge, "solaredge_inverter_vendor_status", "Vendor status")]
    [InfluxDbMetric("vendor_status")]
    public ushort VendorStatus { get; init; }
}
