using System.Text.Json.Serialization;

namespace WebsiteScreenshotWorld.InstantSnap;

/// <summary>Keep the page source alongside the capture.</summary>
public sealed class PageSourceOptions
{
    /// <summary>The format to keep. Defaults to <see cref="PageSourceFormat.Html"/>.</summary>
    [JsonPropertyName("format")]
    public PageSourceFormat? Format { get; set; }
}
