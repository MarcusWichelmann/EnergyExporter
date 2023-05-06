using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EnergyExporter.Options;

public class ExportOptions {
    public const string Key = "Export";

    public bool IndentedJson { get; init; }

    [JsonPropertyName("InfluxDB")]
    public InfluxDbOptions? InfluxDb { get; init; }

    public class InfluxDbOptions {
        [Required]
        public string Url { get; init; } = null!;

        [Required]
        public string Bucket { get; init; } = null!;

        [Required]
        public string Organisation { get; init; } = null!;

        [Required]
        public string Token { get; init; } = null!;
    }
}
