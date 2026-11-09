using System.Text.Json.Serialization;

namespace WebsiteScreenshotWorld.AutoSchedule;

/// <summary>The answer to a trigger-now request. The capture's result arrives later by webhook.</summary>
public sealed class TriggerResult
{
    /// <summary>Whether a capture was queued or one was already pending.</summary>
    [JsonPropertyName("status")]
    public TriggerStatus Status { get; init; }

    /// <summary>The web page, the same id webhooks call <c>schedule_id</c>.</summary>
    [JsonPropertyName("web_page_id")]
    public Guid WebPageId { get; init; }

    /// <summary>Who started the pending capture.</summary>
    [JsonPropertyName("triggered_by")]
    public TriggeredBy TriggeredBy { get; init; }

    /// <summary>When the capture was queued, in UTC. For an already pending capture, when it was first queued.</summary>
    [JsonPropertyName("triggered_at")]
    public DateTimeOffset TriggeredAt { get; init; }

    /// <summary>One sentence for a person.</summary>
    [JsonPropertyName("message")]
    public string? Message { get; init; }
}
