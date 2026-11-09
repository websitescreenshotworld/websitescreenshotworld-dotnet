using System.Text.Json.Serialization;

namespace WebsiteScreenshotWorld.Webhooks;

/// <summary>An Auto Schedule capture with the outcome of its deliveries.</summary>
public sealed class DeliveredCapture : ScheduledCapture
{
    /// <summary>Each destination the file was sent to.</summary>
    [JsonPropertyName("destinations")]
    public IReadOnlyList<DeliveryDestination> Destinations { get; init; } = [];
}
