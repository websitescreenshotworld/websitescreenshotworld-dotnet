using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebsiteScreenshotWorld.InstantSnap;

/// <summary>
/// The values a capture takes for options it does not set, and the limits it is checked against, versioned by
/// <see cref="SpecVersion"/>.
/// </summary>
public sealed class CaptureDefaults
{
    /// <summary>The version of this set, such as <c>v1</c>.</summary>
    [JsonPropertyName("spec_version")]
    public string? SpecVersion { get; init; }

    /// <summary>The default response type.</summary>
    [JsonPropertyName("response_type")]
    public ResponseType? ResponseType { get; init; }

    /// <summary>The default media category.</summary>
    [JsonPropertyName("media_category")]
    public MediaCategory? MediaCategory { get; init; }

    /// <summary>The default format of each media category, keyed by category.</summary>
    [JsonPropertyName("format")]
    public IReadOnlyDictionary<string, string>? Format { get; init; }

    /// <summary>The formats each media category accepts, keyed by category.</summary>
    [JsonPropertyName("formats")]
    public IReadOnlyDictionary<string, IReadOnlyList<string>>? Formats { get; init; }

    /// <summary>The default device category.</summary>
    [JsonPropertyName("device_category")]
    public DeviceCategory? DeviceCategory { get; init; }

    /// <summary>The device used for mobile captures when none is set.</summary>
    [JsonPropertyName("emulated_device")]
    public string? EmulatedDevice { get; init; }

    /// <summary>The default viewport.</summary>
    [JsonPropertyName("viewport")]
    public Viewport? Viewport { get; init; }

    /// <summary>The default zoom in percent.</summary>
    [JsonPropertyName("zoom")]
    public int? Zoom { get; init; }

    /// <summary>The default scroll mode.</summary>
    [JsonPropertyName("scroll_mode")]
    public ScrollMode? ScrollMode { get; init; }

    /// <summary>Whether captures grow to the page's height by default.</summary>
    [JsonPropertyName("auto_height")]
    public bool? AutoHeight { get; init; }

    /// <summary>The default height limit for auto-height captures.</summary>
    [JsonPropertyName("auto_height_max_height")]
    public int? AutoHeightMaxHeight { get; init; }

    /// <summary>The height limit for auto-height WebP captures.</summary>
    [JsonPropertyName("auto_height_max_height_webp")]
    public int? AutoHeightMaxHeightWebP { get; init; }

    /// <summary>Whether ads are blocked by default.</summary>
    [JsonPropertyName("block_ads")]
    public bool? BlockAds { get; init; }

    /// <summary>Whether pop-ups are blocked by default.</summary>
    [JsonPropertyName("block_pop_ups")]
    public bool? BlockPopUps { get; init; }

    /// <summary>Whether Escape is pressed by default.</summary>
    [JsonPropertyName("press_escape")]
    public bool? PressEscape { get; init; }

    /// <summary>The default page load timeout in milliseconds.</summary>
    [JsonPropertyName("page_timeout_ms")]
    public int? PageTimeoutMs { get; init; }

    /// <summary>The default pause between scroll steps in milliseconds.</summary>
    [JsonPropertyName("scrolling_delay_ms")]
    public int? ScrollingDelayMs { get; init; }

    /// <summary>Whether a geolocation is set by default.</summary>
    [JsonPropertyName("geolocation")]
    public bool? Geolocation { get; init; }

    /// <summary>The default geolocation accuracy in metres.</summary>
    [JsonPropertyName("geolocation_accuracy")]
    public double? GeolocationAccuracy { get; init; }

    /// <summary>Whether bucket uploads are awaited by default.</summary>
    [JsonPropertyName("wait_for_cloud_sync")]
    public bool? WaitForCloudSync { get; init; }

    /// <summary>The timestamp watermark defaults.</summary>
    [JsonPropertyName("timestamp_watermark")]
    public JsonElement? TimestampWatermark { get; init; }

    /// <summary>The page source defaults.</summary>
    [JsonPropertyName("page_source")]
    public JsonElement? PageSource { get; init; }

    /// <summary>The limits options are checked against.</summary>
    [JsonPropertyName("limits")]
    public JsonElement? Limits { get; init; }

    /// <summary>Options whose default is whatever the browser does.</summary>
    [JsonPropertyName("left_to_the_browser")]
    public IReadOnlyList<string>? LeftToTheBrowser { get; init; }

    /// <summary>Fields this version of the library does not know yet.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement>? AdditionalProperties { get; set; }
}
