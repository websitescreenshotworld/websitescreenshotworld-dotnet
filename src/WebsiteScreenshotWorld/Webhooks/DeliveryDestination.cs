using System.Text.Json.Serialization;

namespace WebsiteScreenshotWorld.Webhooks;

/// <summary>One destination a scheduled capture was delivered to.</summary>
public sealed class DeliveryDestination
{
    /// <summary>The destination's slot number on the web page.</summary>
    [JsonPropertyName("slot")]
    public int Slot { get; init; }

    /// <summary>The destination's name in the dashboard.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>The storage provider, such as <c>aws_s3</c>.</summary>
    [JsonPropertyName("provider")]
    public string? Provider { get; init; }

    /// <summary>How the delivery went, such as <c>succeeded</c>.</summary>
    [JsonPropertyName("status")]
    public string? Status { get; init; }

    /// <summary>When the delivery finished, in UTC.</summary>
    [JsonPropertyName("completed_at")]
    public DateTimeOffset? CompletedAt { get; init; }
}
