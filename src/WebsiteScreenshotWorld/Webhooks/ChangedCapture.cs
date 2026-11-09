using System.Text.Json.Serialization;

namespace WebsiteScreenshotWorld.Webhooks;

/// <summary>An Auto Schedule capture that differs from the one before it.</summary>
public sealed class ChangedCapture : ScheduledCapture
{
    /// <summary>How much of the page changed, in percent.</summary>
    [JsonPropertyName("change_percent")]
    public double ChangePercent { get; init; }

    /// <summary>The earlier capture this one was compared with.</summary>
    [JsonPropertyName("compared_to_capture_id")]
    public Guid? ComparedToCaptureId { get; init; }

    /// <summary>Whether an image of the change is available.</summary>
    [JsonPropertyName("change_image_available")]
    public bool ChangeImageAvailable { get; init; }
}
