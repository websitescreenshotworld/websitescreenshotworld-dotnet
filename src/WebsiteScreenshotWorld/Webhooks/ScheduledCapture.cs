using System.Text.Json.Serialization;
using WebsiteScreenshotWorld.InstantSnap;

namespace WebsiteScreenshotWorld.Webhooks;

/// <summary>An Auto Schedule capture, as webhooks describe it.</summary>
public class ScheduledCapture
{
    /// <summary>The capture id.</summary>
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    /// <summary>The web page the capture belongs to; the id trigger-now calls <c>web_page_id</c>.</summary>
    [JsonPropertyName("schedule_id")]
    public Guid ScheduleId { get; init; }

    /// <summary>The web page's name in the dashboard.</summary>
    [JsonPropertyName("schedule_name")]
    public string? ScheduleName { get; init; }

    /// <summary>The page that was captured.</summary>
    [JsonPropertyName("url")]
    public string? Url { get; init; }

    /// <summary>Where the capture is in its life.</summary>
    [JsonPropertyName("status")]
    public ScheduledCaptureStatus Status { get; init; }

    /// <summary>Image, video or PDF.</summary>
    [JsonPropertyName("media_category")]
    public MediaCategory MediaCategory { get; init; }

    /// <summary>The format written.</summary>
    [JsonPropertyName("format")]
    public CaptureFormat? Format { get; init; }

    /// <summary>The HTTP status the website answered with.</summary>
    [JsonPropertyName("page_http_status")]
    public int? PageHttpStatus { get; init; }

    /// <summary>When the page was captured, in UTC.</summary>
    [JsonPropertyName("captured_at")]
    public DateTimeOffset? CapturedAt { get; init; }

    /// <summary>Where the browser ended up, when the page redirected.</summary>
    [JsonPropertyName("final_url")]
    public string? FinalUrl { get; init; }

    /// <summary>The file's SHA-256 digest as lowercase hex.</summary>
    [JsonPropertyName("media_sha256")]
    public string? MediaSha256 { get; init; }

    /// <summary>The page source's format and digest, when it was kept.</summary>
    [JsonPropertyName("page_source")]
    public CapturePageSource? PageSource { get; init; }

    /// <summary>The sitemap run this capture was part of, if any.</summary>
    [JsonPropertyName("sitemap_run_id")]
    public Guid? SitemapRunId { get; init; }
}
