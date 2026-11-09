using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using WebsiteScreenshotWorld.Internal;

namespace WebsiteScreenshotWorld.Webhooks;

/// <summary>Reads a verified delivery body into its typed event.</summary>
internal static class WebhookEventReader
{
    public static WebhookEvent Read(ReadOnlySpan<byte> body)
    {
        try
        {
            var reader = new Utf8JsonReader(body);
            using var document = JsonDocument.ParseValue(ref reader);
            return Read(document.RootElement);
        }
        catch (JsonException exception)
        {
            throw NotAnEvent(exception.Message, exception);
        }
    }

    private static WebhookEvent Read(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Object)
        {
            throw NotAnEvent("the body is not a JSON object.");
        }

        var id = RequiredString(root, "id");
        var type = RequiredString(root, "type");
        var product = new WebhookProduct(RequiredString(root, "product"));
        if (!Iso8601.TryParse(RequiredString(root, "created_at"), out var createdAt))
        {
            throw NotAnEvent("\"created_at\" is not a date and time.");
        }

        if (!root.TryGetProperty("data", out var data))
        {
            throw NotAnEvent("\"data\" is missing.");
        }

        var context = WswJsonContext.Default;
        var isInstantSnap = product == WebhookProduct.InstantSnap;
        var isAutoSchedule = product == WebhookProduct.AutoSchedule;

        return type switch
        {
            "capture.succeeded" when isInstantSnap =>
                new InstantSnapCaptureSucceededEvent(id, createdAt, Data(data, context.Capture)),
            "capture.failed" when isInstantSnap =>
                new InstantSnapCaptureFailedEvent(id, createdAt, Data(data, context.Capture)),
            "capture.succeeded" when isAutoSchedule =>
                new ScheduledCaptureSucceededEvent(id, createdAt, Data(data, context.ScheduledCapture)),
            "capture.failed" when isAutoSchedule =>
                new ScheduledCaptureFailedEvent(id, createdAt, Data(data, context.ScheduledCapture)),
            "delivery.completed" when isAutoSchedule =>
                new DeliveryCompletedEvent(id, createdAt, Data(data, context.DeliveredCapture)),
            "capture.changed" when isAutoSchedule =>
                new CaptureChangedEvent(id, createdAt, Data(data, context.ChangedCapture)),
            "schedule.paused" when isAutoSchedule =>
                new SchedulePausedEvent(id, createdAt, Data(data, context.SchedulePause)),
            "sitemap_run.completed" when isAutoSchedule =>
                new SitemapRunCompletedEvent(id, createdAt, Data(data, context.SitemapRun)),
            "ping" =>
                new PingEvent(id, createdAt, product, Data(data, context.WebhookPing)),
            _ =>
                new UnknownWebhookEvent(id, type, createdAt, product, data.Clone()),
        };
    }

    private static T Data<T>(JsonElement data, JsonTypeInfo<T> typeInfo) =>
        data.ValueKind == JsonValueKind.Object
            ? JsonSerializer.Deserialize(data, typeInfo)!
            : throw NotAnEvent("\"data\" is not a JSON object.");

    private static string RequiredString(JsonElement root, string name) =>
        root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()!
            : throw NotAnEvent($"\"{name}\" is missing.");

    private static WebsiteScreenshotWorldException NotAnEvent(string reason, Exception? cause = null) =>
        new($"The webhook body is not a valid event: {reason}", cause);
}
