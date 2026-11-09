using System.Text.Json.Serialization;

namespace WebsiteScreenshotWorld.Webhooks;

/// <summary>A scheduled web page that stopped being captured.</summary>
public sealed class SchedulePause
{
    /// <summary>The web page; the id trigger-now calls <c>web_page_id</c>.</summary>
    [JsonPropertyName("schedule_id")]
    public Guid ScheduleId { get; init; }

    /// <summary>The web page's name in the dashboard.</summary>
    [JsonPropertyName("schedule_name")]
    public string? ScheduleName { get; init; }

    /// <summary>The page's URL.</summary>
    [JsonPropertyName("url")]
    public string? Url { get; init; }

    /// <summary>Why it was paused, such as <c>quota_exhausted</c>.</summary>
    [JsonPropertyName("reason")]
    public string? Reason { get; init; }

    /// <summary>When it was paused, in UTC.</summary>
    [JsonPropertyName("paused_at")]
    public DateTimeOffset PausedAt { get; init; }
}
