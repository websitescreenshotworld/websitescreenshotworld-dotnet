using System.Text.Json.Serialization;
using WebsiteScreenshotWorld.AutoSchedule;
using WebsiteScreenshotWorld.InstantSnap;
using WebsiteScreenshotWorld.Webhooks;

namespace WebsiteScreenshotWorld.Internal;

[JsonSourceGenerationOptions(
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    Converters = [typeof(LenientDateTimeOffsetConverter)])]
[JsonSerializable(typeof(CaptureOptions))]
[JsonSerializable(typeof(Capture))]
[JsonSerializable(typeof(CaptureDefaults))]
[JsonSerializable(typeof(CaptureCatalog))]
[JsonSerializable(typeof(TriggerResult))]
[JsonSerializable(typeof(ScheduledCapture))]
[JsonSerializable(typeof(DeliveredCapture))]
[JsonSerializable(typeof(ChangedCapture))]
[JsonSerializable(typeof(SchedulePause))]
[JsonSerializable(typeof(SitemapRun))]
[JsonSerializable(typeof(WebhookPing))]
internal sealed partial class WswJsonContext : JsonSerializerContext
{
}
