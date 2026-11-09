using System.Text.Json.Serialization;

namespace WebsiteScreenshotWorld.InstantSnap;

/// <summary>An Instant Snap capture: what was captured, where the file is and how it went.</summary>
public sealed class Capture
{
    /// <summary>The capture id.</summary>
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    /// <summary>
    /// Where the capture is in its life. An async capture from <see cref="InstantSnapClient.SubmitAsync"/> or
    /// <see cref="InstantSnapClient.SubmitToBucketAsync"/> starts <see cref="CaptureStatus.Queued"/>, then
    /// <see cref="CaptureStatus.Running"/>, then succeeds or fails.
    /// </summary>
    [JsonPropertyName("status")]
    public CaptureStatus Status { get; init; }

    /// <summary>How the capture was answered.</summary>
    [JsonPropertyName("response_type")]
    public ResponseType ResponseType { get; init; }

    /// <summary>Image, video or PDF. <see langword="null"/> while an async capture is queued or running.</summary>
    [JsonPropertyName("media_category")]
    public MediaCategory? MediaCategory { get; init; }

    /// <summary>The format written.</summary>
    [JsonPropertyName("format")]
    public CaptureFormat? Format { get; init; }

    /// <summary>The file's MIME type, such as <c>image/png</c>.</summary>
    [JsonPropertyName("content_type")]
    public string? ContentType { get; init; }

    /// <summary>The file size in bytes.</summary>
    [JsonPropertyName("size_bytes")]
    public long? SizeBytes { get; init; }

    /// <summary>The width of the file in pixels.</summary>
    [JsonPropertyName("width")]
    public int? Width { get; init; }

    /// <summary>The height of the file in pixels.</summary>
    [JsonPropertyName("height")]
    public int? Height { get; init; }

    /// <summary>How long the capture took, in milliseconds.</summary>
    [JsonPropertyName("duration_ms")]
    public long? DurationMs { get; init; }

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

    /// <summary>
    /// A link to the file, valid for 60 minutes. <see cref="InstantSnapClient.GetAsync"/> issues a fresh one.
    /// <see langword="null"/> for <c>by_format</c> and <c>cloud_storage</c> captures and in webhooks.
    /// </summary>
    [JsonPropertyName("download_url")]
    public Uri? DownloadUrl { get; init; }

    /// <summary>When the file is deleted (24 hours after the capture), in UTC.</summary>
    [JsonPropertyName("expires_at")]
    public DateTimeOffset? ExpiresAt { get; init; }

    /// <summary>The page source, when it was requested.</summary>
    [JsonPropertyName("page_source")]
    public CapturePageSource? PageSource { get; init; }

    /// <summary>The upload to the caller's bucket, for <c>cloud_storage</c> captures.</summary>
    [JsonPropertyName("s3_delivery")]
    public CaptureS3Delivery? S3Delivery { get; init; }

    /// <summary>The URL that receives this capture's webhook, when one was given.</summary>
    [JsonPropertyName("callback_url")]
    public string? CallbackUrl { get; init; }

    /// <summary>Why a failed or timed-out capture ended, as an error code.</summary>
    [JsonPropertyName("error_code")]
    public string? ErrorCode { get; init; }

    /// <summary>The set of defaults the capture was taken under, such as <c>v1</c>.</summary>
    [JsonPropertyName("spec_version")]
    public string? SpecVersion { get; init; }
}
