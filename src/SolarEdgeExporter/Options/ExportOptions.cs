using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SolarEdgeExporter.Options; 

public class ExportOptions {
    public const string Key = "Export";

    public bool IndentedJson { get; init; }

    [JsonPropertyName("InfluxDB")]
    public InfluxDbOptions? InfluxDb { get; init; }

    public class InfluxDbOptions {
        [Required]
        public string? Url { get; init; }

        [Required]
        public string? Bucket { get; init; }

        [Required]
        public string? Organisation { get; init; }

        [Required]
        public string? Token { get; init; }
    }
}
