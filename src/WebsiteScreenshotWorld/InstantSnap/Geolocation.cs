using System.Text.Json.Serialization;

namespace WebsiteScreenshotWorld.InstantSnap;

/// <summary>The position the browser reports. Set latitude and longitude together.</summary>
public sealed class Geolocation
{
    /// <summary>Latitude in degrees, -90 to 90.</summary>
    [JsonPropertyName("latitude")]
    public double? Latitude { get; set; }

    /// <summary>Longitude in degrees, -180 to 180.</summary>
    [JsonPropertyName("longitude")]
    public double? Longitude { get; set; }

    /// <summary>Accuracy in metres, 1 to 10000. Defaults to 100.</summary>
    [JsonPropertyName("accuracy")]
    public double? Accuracy { get; set; }
}
