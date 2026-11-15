using WebsiteScreenshotWorld.InstantSnap;

namespace WebsiteScreenshotWorld.RenderLinks;

/// <summary>
/// What a render link captures. <see cref="RenderSecret"/>, <see cref="KeyPrefix"/> and <see cref="Url"/> are
/// required; an option left <see langword="null"/> is not in the link, and the API applies its default.
/// </summary>
public sealed class RenderLinkOptions
{
    /// <summary>The key's render secret (<c>wsw_rsk_…</c>), shown on the API Keys page once render links are on for the key.</summary>
    public string? RenderSecret { get; set; }

    /// <summary>The key's 12-character prefix: the part after <c>wsw_api_</c> and before the next <c>_</c>.</summary>
    public string? KeyPrefix { get; set; }

    /// <summary>The page to capture.</summary>
    public string? Url { get; set; }

    /// <summary><see cref="CaptureFormat.Png"/> (the default), <see cref="CaptureFormat.Jpeg"/> or <see cref="CaptureFormat.WebP"/>.</summary>
    public CaptureFormat? Format { get; set; }

    /// <summary>Defaults to <c>https://api.websitescreenshotworld.com</c>.</summary>
    public Uri? BaseUrl { get; set; }

    /// <summary>
    /// How long, in seconds, a capture of the link is served from cache before a view captures again: 3600 to
    /// 2592000. The server uses 86400 (a day) when it is left out.
    /// </summary>
    public int? TtlSeconds { get; set; }

    /// <summary>The browser viewport's width, in CSS pixels.</summary>
    public int? Width { get; set; }

    /// <summary>The browser viewport's height, in CSS pixels.</summary>
    public int? Height { get; set; }

    /// <summary>The viewport's device scale factor (1 to 3).</summary>
    public double? DeviceScaleFactor { get; set; }

    /// <summary>Desktop or mobile.</summary>
    public DeviceCategory? DeviceCategory { get; set; }

    /// <summary>A device to emulate, by its name in <see cref="InstantSnapClient.GetCatalogAsync"/>.</summary>
    public string? EmulatedDevice { get; set; }

    /// <summary><see cref="InstantSnap.ScrollMode.Scroll"/> for the full page.</summary>
    public ScrollMode? ScrollMode { get; set; }

    /// <summary>Grow the viewport to the page's height.</summary>
    public bool? AutoHeight { get; set; }

    /// <summary>The tallest a full-page capture may be, in pixels.</summary>
    public int? MaxHeight { get; set; }

    /// <summary>The page zoom, in percent.</summary>
    public int? Zoom { get; set; }

    /// <summary>Block ads.</summary>
    public bool? BlockAds { get; set; }

    /// <summary>Close cookie banners and pop-ups.</summary>
    public bool? BlockPopUps { get; set; }

    /// <summary>Press Escape before capturing, for a modal that closes on it.</summary>
    public bool? PressEscape { get; set; }

    /// <summary>How long the page may take to load, in milliseconds.</summary>
    public int? PageTimeoutMs { get; set; }

    /// <summary>The pause between scroll steps of a full-page capture, in milliseconds.</summary>
    public int? ScrollingDelayMs { get; set; }

    /// <summary>The browser's locale, such as <c>en-GB</c>.</summary>
    public string? Locale { get; set; }

    /// <summary>The browser's time zone, such as <c>Europe/London</c>.</summary>
    public string? TimeZone { get; set; }

    /// <summary>The capture's <c>selectors.hide</c>.</summary>
    public string? Hide { get; set; }

    /// <summary>The capture's <c>selectors.xpath</c>.</summary>
    public string? XPath { get; set; }

    /// <summary>The capture's <c>selectors.click</c>.</summary>
    public string? Click { get; set; }

    /// <summary>The capture's <c>selectors.hover</c>.</summary>
    public string? Hover { get; set; }

    /// <summary>The capture's <c>proxy.country</c>.</summary>
    public string? Country { get; set; }

    /// <summary>Shrink the image to this width after the capture (16 to 3840), keeping its proportions.</summary>
    public int? ThumbWidth { get; set; }
}
