using System.Text.Json.Serialization;

namespace WebsiteScreenshotWorld.InstantSnap;

/// <summary>The browser viewport. Unset values take the API's defaults.</summary>
public sealed class Viewport
{
    /// <summary>Width in CSS pixels, 250 to 7680. Defaults to 1920.</summary>
    [JsonPropertyName("width")]
    public int? Width { get; set; }

    /// <summary>Height in CSS pixels, 250 to 7680. Defaults to 1080.</summary>
    [JsonPropertyName("height")]
    public int? Height { get; set; }

    /// <summary>Device pixel ratio, 1 to 3. Defaults to 1.</summary>
    [JsonPropertyName("device_scale_factor")]
    public double? DeviceScaleFactor { get; set; }
}
