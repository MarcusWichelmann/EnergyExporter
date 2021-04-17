using SolarEdgeExporter.Modbus;
using SolarEdgeExporter.Prometheus;

namespace SolarEdgeExporter.Devices
{
    public class Inverter : IDevice
    {
        [StringModbusRegister(4, 32)]
        public string? Manufacturer { get; init; }
        
        [ScaledModbusRegister(83, typeof(short), 84)]
        [PrometheusMetric(MetricType.Gauge, "ac_power", "AC Power value")]
        public double AcPower { get; init; }
    }
}
