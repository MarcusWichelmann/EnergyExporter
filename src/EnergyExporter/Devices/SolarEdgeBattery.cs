using EnergyExporter.InfluxDb;
using EnergyExporter.Modbus;
using EnergyExporter.Prometheus;

namespace EnergyExporter.Devices;

public enum SolarEdgeBatteryStatus : uint
{
    Off = 0,
    Standby = 1,
    Initializing = 2,
    Charging = 3,
    Discharging = 4,
    Fault = 5,
    Idle = 7
}

[InfluxDbMeasurement("solaredge_battery")]
public class SolarEdgeBattery : SolarEdgeDevice
{
    public static readonly ushort[] ModbusAddresses = { 0xE100, 0xE200 };

    /// <inheritdoc />
    public override string DeviceType => "SolarEdgeBattery";

    [StringModbusRegister(0, 32)]
    [InfluxDbMetric("manufacturer")]
    public string? Manufacturer { get; init; }

    [StringModbusRegister(16, 32)]
    [InfluxDbMetric("model")]
    public string? Model { get; init; }

    [StringModbusRegister(32, 32)]
    [InfluxDbMetric("version")]
    public string? Version { get; init; }

    [StringModbusRegister(48, 32)]
    [InfluxDbMetric("serial_number")]
    public override string? SerialNumber { get; set; }

    [ModbusRegister(64, RegisterEndianness.MidLittleEndian)]
    [InfluxDbMetric("device_address")]
    public ushort DeviceAddress { get; init; }

    [ModbusRegister(66, RegisterEndianness.MidLittleEndian)]
    [PrometheusMetric(MetricType.Gauge, "solaredge_battery_rated_capacity", "Rated capacity")]
    [InfluxDbMetric("rated_capacity")]
    public float RatedCapacity { get; init; }

    [ModbusRegister(68, RegisterEndianness.MidLittleEndian)]
    [PrometheusMetric(MetricType.Gauge, "solaredge_battery_max_charge_continuous_power", "Max charge continuous power")]
    [InfluxDbMetric("max_charge_continuous_power")]
    public float MaxChargeContinuousPower { get; init; }

    [ModbusRegister(70, RegisterEndianness.MidLittleEndian)]
    [PrometheusMetric(
        MetricType.Gauge,
        "solaredge_battery_max_discharge_continuous_power",
        "Max discharge continuous power")]
    [InfluxDbMetric("max_discharge_continuous_power")]
    public float MaxDischargeContinuousPower { get; init; }

    [ModbusRegister(72, RegisterEndianness.MidLittleEndian)]
    [PrometheusMetric(MetricType.Gauge, "solaredge_battery_max_charge_peak_power", "Max charge peak power")]
    [InfluxDbMetric("max_charge_peak_power")]
    public float MaxChargePeakPower { get; init; }

    [ModbusRegister(74, RegisterEndianness.MidLittleEndian)]
    [PrometheusMetric(MetricType.Gauge, "solaredge_battery_max_discharge_peak_power", "Max discharge peak power")]
    [InfluxDbMetric("max_discharge_peak_power")]
    public float MaxDischargePeakPower { get; init; }

    [ModbusRegister(108, RegisterEndianness.MidLittleEndian)]
    [PrometheusMetric(MetricType.Gauge, "solaredge_battery_avg_temperature", "Average temperature")]
    [InfluxDbMetric("avg_temperature")]
    public float AvgTemperature { get; init; }

    [ModbusRegister(110, RegisterEndianness.MidLittleEndian)]
    [PrometheusMetric(MetricType.Gauge, "solaredge_battery_max_temperature", "Maximum temperature")]
    [InfluxDbMetric("max_temperature")]
    public float MaxTemperature { get; init; }

    [ModbusRegister(112, RegisterEndianness.MidLittleEndian)]
    [PrometheusMetric(MetricType.Gauge, "solaredge_battery_voltage", "Voltage")]
    [InfluxDbMetric("voltage")]
    public float Voltage { get; init; }

    [ModbusRegister(114, RegisterEndianness.MidLittleEndian)]
    [PrometheusMetric(MetricType.Gauge, "solaredge_battery_current", "Current")]
    [InfluxDbMetric("current")]
    public float Current { get; init; }

    [ModbusRegister(116, RegisterEndianness.MidLittleEndian)]
    [PrometheusMetric(MetricType.Gauge, "solaredge_battery_power", "Power")]
    [InfluxDbMetric("power")]
    public float Power { get; init; }

    [ModbusRegister(118, RegisterEndianness.MidLittleEndian)]
    [PrometheusMetric(MetricType.Counter, "solaredge_battery_lifetime_exported_energy", "Lifetime exported energy")]
    [InfluxDbMetric("lifetime_exported_energy")]
    public ulong LifetimeExportedEnergy { get; init; }

    [ModbusRegister(122, RegisterEndianness.MidLittleEndian)]
    [PrometheusMetric(MetricType.Counter, "solaredge_battery_lifetime_imported_energy", "Lifetime imported energy")]
    [InfluxDbMetric("lifetime_imported_energy")]
    public ulong LifetimeImportedEnergy { get; init; }

    [ModbusRegister(126, RegisterEndianness.MidLittleEndian)]
    [PrometheusMetric(MetricType.Gauge, "solaredge_battery_capacity", "Power")]
    [InfluxDbMetric("capacity")]
    public float Capacity { get; init; }

    [ModbusRegister(128, RegisterEndianness.MidLittleEndian)]
    [PrometheusMetric(MetricType.Gauge, "solaredge_battery_charge", "Charge")]
    [InfluxDbMetric("charge")]
    public float Charge { get; init; }

    [ModbusRegister(130, RegisterEndianness.MidLittleEndian)]
    [PrometheusMetric(MetricType.Gauge, "solaredge_battery_capacity_percent", "Capacity in percent")]
    [InfluxDbMetric("capacity_percent")]
    public float CapacityPercent { get; init; }

    [ModbusRegister(132, RegisterEndianness.MidLittleEndian)]
    [PrometheusMetric(MetricType.Gauge, "solaredge_battery_charge_percent", "Charge in percent")]
    [InfluxDbMetric("charge_percent")]
    public float ChargePercent { get; init; }

    [ModbusRegister(134, RegisterEndianness.MidLittleEndian)]
    [PrometheusMetric(MetricType.Gauge, "solaredge_battery_status", "Status")]
    [InfluxDbMetric("status")]
    public SolarEdgeBatteryStatus Status { get; init; }

    [ModbusRegister(136, RegisterEndianness.MidLittleEndian)]
    [PrometheusMetric(MetricType.Gauge, "solaredge_battery_vendor_status", "Vendor status")]
    [InfluxDbMetric("vendor_status")]
    public uint VendorStatus { get; init; }

    [ModbusRegister(138, RegisterEndianness.MidLittleEndian)]
    [InfluxDbMetric("last_event")]
    public ushort LastEvent { get; init; }
}
