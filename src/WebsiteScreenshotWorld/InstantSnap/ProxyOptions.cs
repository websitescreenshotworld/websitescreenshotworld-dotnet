using System.Text.Json.Serialization;

namespace WebsiteScreenshotWorld.InstantSnap;

/// <summary>Load the page through a proxy in another country.</summary>
public sealed class ProxyOptions
{
    /// <summary>A country from <see cref="CaptureCatalog.ProxyCountries"/>, such as <c>gb</c>.</summary>
    [JsonPropertyName("country")]
    public string? Country { get; set; }
}
