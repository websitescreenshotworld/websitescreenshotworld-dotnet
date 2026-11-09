using System.Text.Json.Serialization;

namespace WebsiteScreenshotWorld.Webhooks;

/// <summary>A finished run over the pages of a sitemap schedule.</summary>
public sealed class SitemapRun
{
    /// <summary>The run id, also carried by each capture of the run.</summary>
    [JsonPropertyName("sitemap_run_id")]
    public Guid SitemapRunId { get; init; }

    /// <summary>The sitemap schedule.</summary>
    [JsonPropertyName("schedule_id")]
    public Guid ScheduleId { get; init; }

    /// <summary>The schedule's name in the dashboard.</summary>
    [JsonPropertyName("schedule_name")]
    public string? ScheduleName { get; init; }

    /// <summary>The sitemap that was read.</summary>
    [JsonPropertyName("sitemap_url")]
    public string? SitemapUrl { get; init; }

    /// <summary>How the run ended, such as <c>completed</c>.</summary>
    [JsonPropertyName("status")]
    public string? Status { get; init; }

    /// <summary>When the run started, in UTC.</summary>
    [JsonPropertyName("run_at")]
    public DateTimeOffset RunAt { get; init; }

    /// <summary>When the run finished, in UTC.</summary>
    [JsonPropertyName("completed_at")]
    public DateTimeOffset? CompletedAt { get; init; }

    /// <summary>Pages the sitemap listed.</summary>
    [JsonPropertyName("pages_listed")]
    public int PagesListed { get; init; }

    /// <summary>Listed pages that matched the schedule.</summary>
    [JsonPropertyName("pages_matched")]
    public int PagesMatched { get; init; }

    /// <summary>Pages queued for capture.</summary>
    [JsonPropertyName("pages_queued")]
    public int PagesQueued { get; init; }

    /// <summary>Pages captured.</summary>
    [JsonPropertyName("pages_succeeded")]
    public int PagesSucceeded { get; init; }

    /// <summary>Pages that could not be captured.</summary>
    [JsonPropertyName("pages_failed")]
    public int PagesFailed { get; init; }

    /// <summary>Matched pages skipped as over the limit.</summary>
    [JsonPropertyName("pages_over_limit")]
    public int PagesOverLimit { get; init; }

    /// <summary>Matched pages skipped as over the plan's quota.</summary>
    [JsonPropertyName("pages_over_quota")]
    public int PagesOverQuota { get; init; }

    /// <summary>A note about the run for a person, if any.</summary>
    [JsonPropertyName("message")]
    public string? Message { get; init; }
}
