using System.Text.Json.Serialization;
using SolarEdgeExporter.Modbus;
using SolarEdgeExporter.Prometheus;

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
        /// <inheritdoc />
        [JsonIgnore]
        public string DeviceIdentifier => SerialNumber!;

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
        [PrometheusMetric(MetricType.Gauge, "solaredge_battery_rated_capacity", "Rated capacity")]
        public float RatedCapacity { get; init; }

        [ModbusRegister(68, RegisterEndianness.MidLittleEndian)]
        [PrometheusMetric(MetricType.Gauge, "solaredge_battery_max_charge_continuous_power", "Max charge continuous power")]
        public float MaxChargeContinuousPower { get; init; }

        [ModbusRegister(70, RegisterEndianness.MidLittleEndian)]
        [PrometheusMetric(MetricType.Gauge, "solaredge_battery_max_discharge_continuous_power", "Max discharge continuous power")]
        public float MaxDischargeContinuousPower { get; init; }

        [ModbusRegister(72, RegisterEndianness.MidLittleEndian)]
        [PrometheusMetric(MetricType.Gauge, "solaredge_battery_max_charge_peak_power", "Max charge peak power")]
        public float MaxChargePeakPower { get; init; }

        [ModbusRegister(74, RegisterEndianness.MidLittleEndian)]
        [PrometheusMetric(MetricType.Gauge, "solaredge_battery_max_discharge_peak_power", "Max discharge peak power")]
        public float MaxDischargePeakPower { get; init; }

        [ModbusRegister(108, RegisterEndianness.MidLittleEndian)]
        [PrometheusMetric(MetricType.Gauge, "solaredge_battery_avg_temperature", "Average temperature")]
        public float AvgTemperature { get; init; }

        [ModbusRegister(110, RegisterEndianness.MidLittleEndian)]
        [PrometheusMetric(MetricType.Gauge, "solaredge_battery_max_temperature", "Maximum temperature")]
        public float MaxTemperature { get; init; }

        [ModbusRegister(112, RegisterEndianness.MidLittleEndian)]
        [PrometheusMetric(MetricType.Gauge, "solaredge_battery_voltage", "Voltage")]
        public float Voltage { get; init; }

        [ModbusRegister(114, RegisterEndianness.MidLittleEndian)]
        [PrometheusMetric(MetricType.Gauge, "solaredge_battery_current", "Current")]
        public float Current { get; init; }

        [ModbusRegister(116, RegisterEndianness.MidLittleEndian)]
        [PrometheusMetric(MetricType.Gauge, "solaredge_battery_power", "Power")]
        public float Power { get; init; }

        [ModbusRegister(118, RegisterEndianness.MidLittleEndian)]
        [PrometheusMetric(MetricType.Counter, "solaredge_battery_lifetime_exported_energy", "Lifetime exported energy")]
        public ulong LifetimeExportedEnergy { get; init; }

        [ModbusRegister(122, RegisterEndianness.MidLittleEndian)]
        [PrometheusMetric(MetricType.Counter, "solaredge_battery_lifetime_imported_energy", "Lifetime imported energy")]
        public ulong LifetimeImportedEnergy { get; init; }

        [ModbusRegister(126, RegisterEndianness.MidLittleEndian)]
        [PrometheusMetric(MetricType.Gauge, "solaredge_battery_capacity", "Power")]
        public float Capacity { get; init; }

        [ModbusRegister(128, RegisterEndianness.MidLittleEndian)]
        [PrometheusMetric(MetricType.Gauge, "solaredge_battery_charge", "Charge")]
        public float Charge { get; init; }

        [ModbusRegister(130, RegisterEndianness.MidLittleEndian)]
        [PrometheusMetric(MetricType.Gauge, "solaredge_battery_capacity_percent", "Capacity in percent")]
        public float CapacityPercent { get; init; }

        [ModbusRegister(132, RegisterEndianness.MidLittleEndian)]
        [PrometheusMetric(MetricType.Gauge, "solaredge_battery_charge_percent", "Charge in percent")]
        public float ChargePercent { get; init; }

        [ModbusRegister(134, RegisterEndianness.MidLittleEndian)]
        [PrometheusMetric(MetricType.Gauge, "solaredge_battery_status", "Status")]
        public BatteryStatus Status { get; init; }

        [ModbusRegister(136, RegisterEndianness.MidLittleEndian)]
        [PrometheusMetric(MetricType.Gauge, "solaredge_battery_vendor_status", "Vendor status")]
        public uint VendorStatus { get; init; }

        [ModbusRegister(138, RegisterEndianness.MidLittleEndian)]
        public ushort LastEvent { get; init; }
    }
}
