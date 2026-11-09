using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebsiteScreenshotWorld.Tests.Support;

/// <summary>Compares JSON the way the spec's fixtures describe.</summary>
internal static class JsonAssert
{
    private static readonly JsonSerializerOptions WireOptions = new()
    {
        Converters = { new UtcDateTimeOffsetConverter() },
    };

    /// <summary>Serialises a result with its wire names, keeping nulls.</summary>
    public static JsonElement ToWire<T>(T value) => JsonSerializer.SerializeToElement(value, WireOptions);

    /// <summary>Every field of <paramref name="expected"/> appears in <paramref name="actual"/> with the same value.</summary>
    public static void Subset(JsonElement expected, JsonElement actual, string path = "$")
    {
        if (expected.ValueKind != JsonValueKind.Object)
        {
            Equal(expected, actual, path);
            return;
        }

        Assert.True(actual.ValueKind == JsonValueKind.Object, $"{path}: expected an object, got {actual.ValueKind}");
        foreach (var field in expected.EnumerateObject())
        {
            Assert.True(actual.TryGetProperty(field.Name, out var value), $"{path}.{field.Name} is missing");
            Subset(field.Value, value, $"{path}.{field.Name}");
        }
    }

    /// <summary>The two values are the same JSON value.</summary>
    public static void Equal(JsonElement expected, JsonElement actual, string path = "$")
    {
        Assert.True(expected.ValueKind == actual.ValueKind, $"{path}: expected {expected}, got {actual}");
        switch (expected.ValueKind)
        {
            case JsonValueKind.Object:
                var expectedNames = expected.EnumerateObject().Select(p => p.Name).Order().ToArray();
                var actualNames = actual.EnumerateObject().Select(p => p.Name).Order().ToArray();
                Assert.True(expectedNames.SequenceEqual(actualNames), $"{path}: expected fields [{string.Join(", ", expectedNames)}], got [{string.Join(", ", actualNames)}]");
                foreach (var field in expected.EnumerateObject())
                {
                    Equal(field.Value, actual.GetProperty(field.Name), $"{path}.{field.Name}");
                }

                break;
            case JsonValueKind.Array:
                Assert.True(expected.GetArrayLength() == actual.GetArrayLength(), $"{path}: array lengths differ");
                var index = 0;
                foreach (var (left, right) in expected.EnumerateArray().Zip(actual.EnumerateArray()))
                {
                    Equal(left, right, $"{path}[{index++}]");
                }

                break;
            case JsonValueKind.Number:
                Assert.True(expected.GetDecimal() == actual.GetDecimal(), $"{path}: expected {expected}, got {actual}");
                break;
            case JsonValueKind.String:
                Assert.True(expected.GetString() == actual.GetString(), $"{path}: expected {expected}, got {actual}");
                break;
        }
    }

    private sealed class UtcDateTimeOffsetConverter : JsonConverter<DateTimeOffset>
    {
        public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            reader.GetDateTimeOffset();

        public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options) =>
            writer.WriteStringValue(value.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture));
    }
}
