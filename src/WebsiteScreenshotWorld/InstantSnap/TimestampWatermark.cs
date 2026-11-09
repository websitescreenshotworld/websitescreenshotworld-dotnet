using System.Text.Json.Serialization;

namespace WebsiteScreenshotWorld.InstantSnap;

/// <summary>A strip showing when and where the capture was taken.</summary>
public sealed class TimestampWatermark
{
    /// <summary>Whether to add the strip. Defaults to <see langword="true"/> once this block is set.</summary>
    [JsonPropertyName("enabled")]
    public bool? Enabled { get; set; }

    /// <summary>Above or below the page. Defaults to <see cref="WatermarkLocation.Top"/>.</summary>
    [JsonPropertyName("location")]
    public WatermarkLocation? Location { get; set; }

    /// <summary>Text colour as hex, such as <c>#000000</c> (the default).</summary>
    [JsonPropertyName("font_color")]
    public string? FontColor { get; set; }

    /// <summary>Background colour as hex, such as <c>#FFFFFF</c> (the default).</summary>
    [JsonPropertyName("background_color")]
    public string? BackgroundColor { get; set; }
}
