using System.Text.Json.Serialization;
using EnergyExporter.InfluxDb;
using EnergyExporter.Modbus;
using EnergyExporter.Prometheus;

namespace EnergyExporter.Devices;

// https://www.janitza.de/files/download/manuals/current/UMG96-PA/firmware-v3/janitza-mal-umg96pa-fw3-de.pdf
// https://www.janitza.de/files/download/manuals/current/UMG96-PA/firmware-v3/janitza-mal-umg96pa-fw3-en.pdf

[InfluxDbMeasurement("janitza_power_analyzer")]
public class JanitzaPowerAnalyzer : JanitzaDevice
{
    public const ushort ModbusAddress = 0;

    /// <inheritdoc />
    public override string DeviceType => "JanitzaPowerAnalyzer";

    [ModbusRegister(911)]
    [InfluxDbMetric("serial_number")]
    public uint SerialNumber { get; init; }

    [ModbusRegister(19000)]
    [PrometheusMetric(MetricType.Gauge, "janitza_power_analyzer_voltage_p1", "Voltage on phase 1")]
    [InfluxDbMetric("voltage_p1")]
    public float VoltageP1 { get; init; }

    [ModbusRegister(19002)]
    [PrometheusMetric(MetricType.Gauge, "janitza_power_analyzer_voltage_p2", "Voltage on phase 2")]
    [InfluxDbMetric("voltage_p2")]
    public float VoltageP2 { get; init; }

    [ModbusRegister(19004)]
    [PrometheusMetric(MetricType.Gauge, "janitza_power_analyzer_voltage_p3", "Voltage on phase 3")]
    [InfluxDbMetric("voltage_p3")]
    public float VoltageP3 { get; init; }

    [ModbusRegister(19006)]
    [PrometheusMetric(MetricType.Gauge, "janitza_power_analyzer_voltage_p1_to_p2", "Voltage between phase 1 and 2")]
    [InfluxDbMetric("voltage_p1_to_p2")]
    public float VoltageP1ToP2 { get; init; }

    [ModbusRegister(19008)]
    [PrometheusMetric(MetricType.Gauge, "janitza_power_analyzer_voltage_p2_to_p3", "Voltage between phase 2 and 3")]
    [InfluxDbMetric("voltage_p2_to_p3")]
    public float VoltageP2ToP3 { get; init; }

    [ModbusRegister(19010)]
    [PrometheusMetric(MetricType.Gauge, "janitza_power_analyzer_voltage_p3_to_p1", "Voltage between phase 3 and 1")]
    [InfluxDbMetric("voltage_p3_to_p1")]
    public float VoltageP3ToP1 { get; init; }

    [ModbusRegister(19012)]
    [PrometheusMetric(MetricType.Gauge, "janitza_power_analyzer_current_p1", "Current on phase 1")]
    [InfluxDbMetric("current_p1")]
    public float CurrentP1 { get; init; }

    [ModbusRegister(19014)]
    [PrometheusMetric(MetricType.Gauge, "janitza_power_analyzer_current_p2", "Current on phase 2")]
    [InfluxDbMetric("current_p2")]
    public float CurrentP2 { get; init; }

    [ModbusRegister(19016)]
    [PrometheusMetric(MetricType.Gauge, "janitza_power_analyzer_current_p3", "Current on phase 3")]
    [InfluxDbMetric("current_p3")]
    public float CurrentP3 { get; init; }

    [ModbusRegister(19018)]
    [PrometheusMetric(MetricType.Gauge, "janitza_power_analyzer_current_sum", "Vector sum of current")]
    [InfluxDbMetric("current_sum")]
    public float CurrentSum { get; init; }

    [ModbusRegister(19020)]
    [PrometheusMetric(MetricType.Gauge, "janitza_power_analyzer_real_power_p1", "Real power on phase 1")]
    [InfluxDbMetric("real_power_p1")]
    public float RealPowerP1 { get; init; }

    [ModbusRegister(19022)]
    [PrometheusMetric(MetricType.Gauge, "janitza_power_analyzer_real_power_p2", "Real power on phase 2")]
    [InfluxDbMetric("real_power_p2")]
    public float RealPowerP2 { get; init; }

    [ModbusRegister(19024)]
    [PrometheusMetric(MetricType.Gauge, "janitza_power_analyzer_real_power_p3", "Real power on phase 3")]
    [InfluxDbMetric("real_power_p3")]
    public float RealPowerP3 { get; init; }

    [ModbusRegister(19026)]
    [PrometheusMetric(MetricType.Gauge, "janitza_power_analyzer_real_power_sum", "Sum of real power")]
    [InfluxDbMetric("real_power_sum")]
    public float RealPowerSum { get; init; }

    [ModbusRegister(19028)]
    [PrometheusMetric(MetricType.Gauge, "janitza_power_analyzer_apparent_power_p1", "Apparent power on phase 1")]
    [InfluxDbMetric("apparent_power_p1")]
    public float ApparentPowerP1 { get; init; }

    [ModbusRegister(19030)]
    [PrometheusMetric(MetricType.Gauge, "janitza_power_analyzer_apparent_power_p2", "Apparent power on phase 2")]
    [InfluxDbMetric("apparent_power_p2")]
    public float ApparentPowerP2 { get; init; }

    [ModbusRegister(19032)]
    [PrometheusMetric(MetricType.Gauge, "janitza_power_analyzer_apparent_power_p3", "Apparent power on phase 3")]
    [InfluxDbMetric("apparent_power_p3")]
    public float ApparentPowerP3 { get; init; }

    [ModbusRegister(19034)]
    [PrometheusMetric(MetricType.Gauge, "janitza_power_analyzer_apparent_power_sum", "Sum of apparent power")]
    [InfluxDbMetric("apparent_power_sum")]
    public float ApparentPowerSum { get; init; }

    [ModbusRegister(19036)]
    [PrometheusMetric(MetricType.Gauge, "janitza_power_analyzer_reactive_power_p1", "Reactive power on phase 1")]
    [InfluxDbMetric("reactive_power_p1")]
    public float ReactivePowerP1 { get; init; }

    [ModbusRegister(19038)]
    [PrometheusMetric(MetricType.Gauge, "janitza_power_analyzer_reactive_power_p2", "Reactive power on phase 2")]
    [InfluxDbMetric("reactive_power_p2")]
    public float ReactivePowerP2 { get; init; }

    [ModbusRegister(19040)]
    [PrometheusMetric(MetricType.Gauge, "janitza_power_analyzer_reactive_power_p3", "Reactive power on phase 3")]
    [InfluxDbMetric("reactive_power_p3")]
    public float ReactivePowerP3 { get; init; }

    [ModbusRegister(19042)]
    [PrometheusMetric(MetricType.Gauge, "janitza_power_analyzer_reactive_power_sum", "Sum of reactive power")]
    [InfluxDbMetric("reactive_power_sum")]
    public float ReactivePowerSum { get; init; }

    [ModbusRegister(19044)]
    [PrometheusMetric(MetricType.Gauge, "janitza_power_analyzer_power_factor_p1",
        "Fundamental power factor on phase 1")]
    [InfluxDbMetric("power_factor_p1")]
    public float PowerFactorP1 { get; init; }

    [ModbusRegister(19046)]
    [PrometheusMetric(MetricType.Gauge, "janitza_power_analyzer_power_factor_p2",
        "Fundamental power factor on phase 2")]
    [InfluxDbMetric("power_factor_p2")]
    public float PowerFactorP2 { get; init; }

    [ModbusRegister(19048)]
    [PrometheusMetric(MetricType.Gauge, "janitza_power_analyzer_power_factor_p3",
        "Fundamental power factor on phase 3")]
    [InfluxDbMetric("power_factor_p3")]
    public float PowerFactorP3 { get; init; }

    [ModbusRegister(19050)]
    [PrometheusMetric(MetricType.Gauge, "janitza_power_analyzer_frequency", "Measured frequency")]
    [InfluxDbMetric("frequency")]
    public float Frequency { get; init; }

    [ModbusRegister(19052)]
    [PrometheusMetric(MetricType.Gauge, "janitza_power_analyzer_rotation_field",
        "Phase rotation field: 1=right, 0=none, -1=left")]
    [InfluxDbMetric("rotation_field")]
    public float RotationField { get; init; }

    [ModbusRegister(19062)]
    [PrometheusMetric(MetricType.Counter, "janitza_power_analyzer_real_energy_consumed_p1",
        "Real energy consumed on phase 1")]
    [InfluxDbMetric("real_energy_consumed_p1")]
    public float RealEnergyConsumedP1 { get; init; }

    [ModbusRegister(19064)]
    [PrometheusMetric(MetricType.Counter, "janitza_power_analyzer_real_energy_consumed_p2",
        "Real energy consumed on phase 2")]
    [InfluxDbMetric("real_energy_consumed_p2")]
    public float RealEnergyConsumedP2 { get; init; }

    [ModbusRegister(19066)]
    [PrometheusMetric(MetricType.Counter, "janitza_power_analyzer_real_energy_consumed_p3",
        "Real energy consumed on phase 3")]
    [InfluxDbMetric("real_energy_consumed_p3")]
    public float RealEnergyConsumedP3 { get; init; }

    [ModbusRegister(19068)]
    [PrometheusMetric(MetricType.Counter, "janitza_power_analyzer_real_energy_consumed_sum",
        "Sum of real energy consumed")]
    [InfluxDbMetric("real_energy_consumed_sum")]
    public float RealEnergyConsumedSum { get; init; }

    [ModbusRegister(19070)]
    [PrometheusMetric(MetricType.Counter, "janitza_power_analyzer_real_energy_delivered_p1",
        "Real energy delivered on phase 1")]
    [InfluxDbMetric("real_energy_delivered_p1")]
    public float RealEnergyDeliveredP1 { get; init; }

    [ModbusRegister(19072)]
    [PrometheusMetric(MetricType.Counter, "janitza_power_analyzer_real_energy_delivered_p2",
        "Real energy delivered on phase 2")]
    [InfluxDbMetric("real_energy_delivered_p2")]
    public float RealEnergyDeliveredP2 { get; init; }

    [ModbusRegister(19074)]
    [PrometheusMetric(MetricType.Counter, "janitza_power_analyzer_real_energy_delivered_p3",
        "Real energy delivered on phase 3")]
    [InfluxDbMetric("real_energy_delivered_p3")]
    public float RealEnergyDeliveredP3 { get; init; }

    [ModbusRegister(19076)]
    [PrometheusMetric(MetricType.Counter, "janitza_power_analyzer_real_energy_delivered_sum",
        "Sum of real energy delivered")]
    [InfluxDbMetric("real_energy_delivered_sum")]
    public float RealEnergyDeliveredSum { get; init; }

    [ModbusRegister(19078)]
    [PrometheusMetric(MetricType.Counter, "janitza_power_analyzer_apparent_energy_p1", "Apparent energy on phase 1")]
    [InfluxDbMetric("apparent_energy_p1")]
    public float ApparentEnergyP1 { get; init; }

    [ModbusRegister(19080)]
    [PrometheusMetric(MetricType.Counter, "janitza_power_analyzer_apparent_energy_p2", "Apparent energy on phase 2")]
    [InfluxDbMetric("apparent_energy_p2")]
    public float ApparentEnergyP2 { get; init; }

    [ModbusRegister(19082)]
    [PrometheusMetric(MetricType.Counter, "janitza_power_analyzer_apparent_energy_p3", "Apparent energy on phase 3")]
    [InfluxDbMetric("apparent_energy_p3")]
    public float ApparentEnergyP3 { get; init; }

    [ModbusRegister(19084)]
    [PrometheusMetric(MetricType.Counter, "janitza_power_analyzer_apparent_energy_sum", "Sum of apparent energy")]
    [InfluxDbMetric("apparent_energy_sum")]
    public float ApparentEnergySum { get; init; }

    [ModbusRegister(19094)]
    [PrometheusMetric(MetricType.Counter, "janitza_power_analyzer_reactive_energy_inductive_p1",
        "Reactive energy, inductive, on phase 1")]
    [InfluxDbMetric("reactive_energy_inductive_p1")]
    public float ReactiveEnergyInductiveP1 { get; init; }

    [ModbusRegister(19096)]
    [PrometheusMetric(MetricType.Counter, "janitza_power_analyzer_reactive_energy_inductive_p2",
        "Reactive energy, inductive, on phase 2")]
    [InfluxDbMetric("reactive_energy_inductive_p2")]
    public float ReactiveEnergyInductiveP2 { get; init; }

    [ModbusRegister(19098)]
    [PrometheusMetric(MetricType.Counter, "janitza_power_analyzer_reactive_energy_inductive_p3",
        "Reactive energy, inductive, on phase 3")]
    [InfluxDbMetric("reactive_energy_inductive_p3")]
    public float ReactiveEnergyInductiveP3 { get; init; }

    [ModbusRegister(19100)]
    [PrometheusMetric(MetricType.Counter, "janitza_power_analyzer_reactive_energy_inductive_sum",
        "Sum of reactive energy, inductive")]
    [InfluxDbMetric("reactive_energy_inductive_sum")]
    public float ReactiveEnergyInductiveSum { get; init; }

    [ModbusRegister(19102)]
    [PrometheusMetric(MetricType.Counter, "janitza_power_analyzer_reactive_energy_capacitive_p1",
        "Reactive energy, capacitive, on phase 1")]
    [InfluxDbMetric("reactive_energy_capacitive_p1")]
    public float ReactiveEnergyCapacitiveP1 { get; init; }

    [ModbusRegister(19104)]
    [PrometheusMetric(MetricType.Counter, "janitza_power_analyzer_reactive_energy_capacitive_p2",
        "Reactive energy, capacitive, on phase 2")]
    [InfluxDbMetric("reactive_energy_capacitive_p2")]
    public float ReactiveEnergyCapacitiveP2 { get; init; }

    [ModbusRegister(19106)]
    [PrometheusMetric(MetricType.Counter, "janitza_power_analyzer_reactive_energy_capacitive_p3",
        "Reactive energy, capacitive, on phase 3")]
    [InfluxDbMetric("reactive_energy_capacitive_p3")]
    public float ReactiveEnergyCapacitiveP3 { get; init; }

    [ModbusRegister(19108)]
    [PrometheusMetric(MetricType.Counter, "janitza_power_analyzer_reactive_energy_capacitive_sum",
        "Sum of reactive energy, capacitive")]
    [InfluxDbMetric("reactive_energy_capacitive_sum")]
    public float ReactiveEnergyCapacitiveSum { get; init; }

    /// <inheritdoc />
    [JsonIgnore]
    public override string DeviceIdentifier => SerialNumber.ToString();
}
