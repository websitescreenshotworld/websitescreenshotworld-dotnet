using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using WebsiteScreenshotWorld.InstantSnap;

namespace WebsiteScreenshotWorld.Internal;

/// <summary>Writes request bodies and reads JSON answers.</summary>
internal static class JsonBodies
{
    public static byte[] CaptureRequest(CaptureOptions options, ResponseType responseType, bool isAsync)
    {
        var fields = JsonSerializer.SerializeToElement(options, WswJsonContext.Default.CaptureOptions);

        using var buffer = new MemoryStream();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartObject();
            foreach (var field in fields.EnumerateObject())
            {
                field.WriteTo(writer);
            }

            writer.WriteString("response_type", responseType.ToString());
            if (isAsync)
            {
                writer.WriteBoolean("async", true);
            }

            writer.WriteEndObject();
        }

        return buffer.ToArray();
    }

    public static async Task<T> ReadAsync<T>(HttpResponseMessage response, JsonTypeInfo<T> typeInfo, CancellationToken cancellationToken)
    {
        using (response)
        {
            var stream = await response.Content.ReadAsStreamCompatAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var value = await JsonSerializer.DeserializeAsync(stream, typeInfo, cancellationToken).ConfigureAwait(false);
                return value ?? throw Unreadable(response, null);
            }
            catch (JsonException exception)
            {
                throw Unreadable(response, exception);
            }
        }
    }

    private static WebsiteScreenshotWorldException Unreadable(HttpResponseMessage response, Exception? cause) =>
        new($"The API answered with HTTP {(int)response.StatusCode}, but the body was not the expected JSON.", cause);
}
