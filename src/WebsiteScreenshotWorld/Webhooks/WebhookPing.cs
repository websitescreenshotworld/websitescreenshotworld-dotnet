using System.Text.Json.Serialization;

namespace WebsiteScreenshotWorld.Webhooks;

/// <summary>A test delivery sent from the dashboard to check an endpoint.</summary>
public sealed class WebhookPing
{
    /// <summary>The webhook endpoint.</summary>
    [JsonPropertyName("endpoint_id")]
    public Guid EndpointId { get; init; }

    /// <summary>The endpoint's name in the dashboard.</summary>
    [JsonPropertyName("endpoint_name")]
    public string? EndpointName { get; init; }

    /// <summary>A note for a person.</summary>
    [JsonPropertyName("message")]
    public string? Message { get; init; }

    /// <summary>When the ping was sent, in UTC.</summary>
    [JsonPropertyName("sent_at")]
    public DateTimeOffset SentAt { get; init; }
}
