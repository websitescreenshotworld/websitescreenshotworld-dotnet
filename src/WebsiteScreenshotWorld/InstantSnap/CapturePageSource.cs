using System.Text.Json.Serialization;

namespace WebsiteScreenshotWorld.InstantSnap;

/// <summary>The page source kept with a capture.</summary>
public sealed class CapturePageSource
{
    /// <summary>The format it was kept in.</summary>
    [JsonPropertyName("format")]
    public PageSourceFormat Format { get; init; }

    /// <summary>Its MIME type, such as <c>text/html</c>.</summary>
    [JsonPropertyName("content_type")]
    public string? ContentType { get; init; }

    /// <summary>Its size in bytes.</summary>
    [JsonPropertyName("size_bytes")]
    public long? SizeBytes { get; init; }

    /// <summary>Its SHA-256 digest as lowercase hex.</summary>
    [JsonPropertyName("sha256")]
    public string? Sha256 { get; init; }

    /// <summary>A short-lived link to it, issued afresh on every read. Not included in webhooks.</summary>
    [JsonPropertyName("download_url")]
    public Uri? DownloadUrl { get; init; }
}
