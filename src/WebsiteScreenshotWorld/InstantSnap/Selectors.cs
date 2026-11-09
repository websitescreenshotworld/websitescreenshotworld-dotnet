using System.Text.Json.Serialization;

namespace WebsiteScreenshotWorld.InstantSnap;

/// <summary>Elements to act on before capturing.</summary>
public sealed class Selectors
{
    /// <summary>Capture only the element this XPath selects. Up to 512 characters.</summary>
    [JsonPropertyName("xpath")]
    public string? XPath { get; set; }

    /// <summary>
    /// Elements to hide: up to 25 comma-separated CSS selectors (an XPath starting with <c>//</c> works too), up to
    /// 2000 characters. Each is applied on its own; one the browser rejects is skipped.
    /// </summary>
    [JsonPropertyName("hide")]
    public string? Hide { get; set; }

    /// <summary>CSS selector of an element to click. Up to 512 characters.</summary>
    [JsonPropertyName("click")]
    public string? Click { get; set; }

    /// <summary>CSS selector of an element to hover over. Up to 512 characters.</summary>
    [JsonPropertyName("hover")]
    public string? Hover { get; set; }
}
