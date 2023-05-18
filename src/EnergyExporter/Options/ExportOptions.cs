using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EnergyExporter.Options;

public class ExportOptions
{
    public const string Key = "Export";

    public bool IndentedJson { get; init; }

    [JsonPropertyName("InfluxDB")]
    public InfluxDbOptions? InfluxDb { get; init; }

    [JsonPropertyName("Mqtt")]
    public MqttOptions? Mqtt { get; init; }

    public class InfluxDbOptions
    {
        [Required]
        public string Url { get; init; } = null!;

        [Required]
        public string Bucket { get; init; } = null!;

        [Required]
        public string Organisation { get; init; } = null!;

        [Required]
        public string Token { get; init; } = null!;
    }

    public class MqttOptions
    {
        [Required]
        public string TcpServer { get; init; } = null!;

        public int Port { get; init; } = 1883;

        public string ClientId { get; init; } = "EnergyExporter";
        
        public string Topic { get; init; } = "EnergyExporter";
        
        public string? User { get; init; } = null;
        
        public string? Password { get; init; } = null;
    }
}
