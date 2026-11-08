using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EnergyExporter.Utils;

/// <summary>
/// Writes values that a device reports as "not implemented" (NaN) as JSON null. Without this,
/// System.Text.Json refuses to serialize them.
/// </summary>
public class NaNAsNullDoubleConverter : JsonConverter<double>
{
    public override bool HandleNull => true;

    public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType == JsonTokenType.Null ? double.NaN : reader.GetDouble();

    public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
    {
        if (double.IsFinite(value))
            writer.WriteNumberValue(value);
        else
            writer.WriteNullValue();
    }
}
