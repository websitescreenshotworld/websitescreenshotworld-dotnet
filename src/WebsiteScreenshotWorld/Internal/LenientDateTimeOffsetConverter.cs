using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebsiteScreenshotWorld.Internal;

/// <summary>Reads timestamps with any fraction length and either <c>Z</c> or an offset, as UTC.</summary>
internal sealed class LenientDateTimeOffsetConverter : JsonConverter<DateTimeOffset>
{
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String && Iso8601.TryParse(reader.GetString(), out var value))
        {
            return value;
        }

        throw new JsonException("Expected an ISO 8601 date and time.");
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture));
    }
}
