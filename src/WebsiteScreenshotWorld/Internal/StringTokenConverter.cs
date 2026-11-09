using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebsiteScreenshotWorld.Internal;

/// <summary>Reads and writes a string-backed token such as a capture format.</summary>
internal abstract class StringTokenConverter<T> : JsonConverter<T>
    where T : struct
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Expected a string for {typeof(T).Name}.");
        }

        return Create(reader.GetString()!);
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }

    protected abstract T Create(string value);
}
