using System.Text.Json.Serialization;

namespace WebsiteScreenshotWorld.InstantSnap;

/// <summary>The values <see cref="CaptureOptions.EmulatedDevice"/> and <see cref="ProxyOptions.Country"/> accept.</summary>
public sealed class CaptureCatalog
{
    /// <summary>Device names, such as <c>iPhone 13</c>.</summary>
    [JsonPropertyName("emulated_devices")]
    public IReadOnlyList<string> EmulatedDevices { get; init; } = [];

    /// <summary>Proxy country codes, such as <c>gb</c>.</summary>
    [JsonPropertyName("proxy_countries")]
    public IReadOnlyList<string> ProxyCountries { get; init; } = [];
}
