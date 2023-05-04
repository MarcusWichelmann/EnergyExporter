using System;

namespace SolarEdgeExporter.Prometheus; 

public enum MetricType {
    Counter,
    Gauge
}

public static class MetricTypeExtensions {
    public static string ToTypeName(this MetricType metricType) {
        return metricType switch {
            MetricType.Counter => "counter",
            MetricType.Gauge => "gauge",
            var _ => throw new ArgumentOutOfRangeException(nameof(metricType), metricType, null)
        };
    }
}
