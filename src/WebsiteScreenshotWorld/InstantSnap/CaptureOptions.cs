using System.Text.Json.Serialization;

namespace WebsiteScreenshotWorld.InstantSnap;

/// <summary>What to capture and how. Only <see cref="Url"/> is required.</summary>
/// <remarks>
/// A property left <see langword="null"/> is not sent, and the API applies its default (see
/// <see cref="InstantSnapClient.GetDefaultsAsync"/>). <see langword="false"/>, <c>0</c> and empty strings are
/// values and are sent.
/// </remarks>
public sealed class CaptureOptions
{
    /// <summary>The page to capture: an absolute http or https URL without credentials, up to 2048 characters.</summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    /// <summary>Image, video or PDF. Defaults to <see cref="MediaCategory.Image"/>.</summary>
    [JsonPropertyName("media_category")]
    public MediaCategory? MediaCategory { get; set; }

    /// <summary>The file format, which must belong to <see cref="MediaCategory"/>.</summary>
    [JsonPropertyName("format")]
    public CaptureFormat? Format { get; set; }

    /// <summary>The browser viewport.</summary>
    [JsonPropertyName("viewport")]
    public Viewport? Viewport { get; set; }

    /// <summary>Desktop or mobile. Defaults to <see cref="DeviceCategory.Desktop"/>.</summary>
    [JsonPropertyName("device_category")]
    public DeviceCategory? DeviceCategory { get; set; }

    /// <summary>
    /// A device from <see cref="InstantSnapClient.GetCatalogAsync"/>, case-insensitive. Defaults to
    /// <c>iPhone 13</c> when <see cref="DeviceCategory"/> is mobile.
    /// </summary>
    [JsonPropertyName("emulated_device")]
    public string? EmulatedDevice { get; set; }

    /// <summary>
    /// <see cref="InstantSnap.ScrollMode.Scroll"/> captures the full page. Defaults to
    /// <see cref="InstantSnap.ScrollMode.Viewport"/>, or to scroll for video.
    /// </summary>
    [JsonPropertyName("scroll_mode")]
    public ScrollMode? ScrollMode { get; set; }

    /// <summary>Grow the capture to the page's height. Defaults to <see langword="false"/>.</summary>
    [JsonPropertyName("auto_height")]
    public bool? AutoHeight { get; set; }

    /// <summary>
    /// The tallest capture allowed, 250 to 100000 pixels. A value above the format's ceiling (WebP 16383
    /// divided by the scale factor, JPEG 65500) is refused rather than lowered.
    /// </summary>
    [JsonPropertyName("max_height")]
    public int? MaxHeight { get; set; }

    /// <summary>Page zoom in percent, 50 to 100. Defaults to 100.</summary>
    [JsonPropertyName("zoom")]
    public int? Zoom { get; set; }

    /// <summary>Block ads. Defaults to <see langword="true"/>.</summary>
    [JsonPropertyName("block_ads")]
    public bool? BlockAds { get; set; }

    /// <summary>Block pop-ups. Defaults to <see langword="true"/>.</summary>
    [JsonPropertyName("block_pop_ups")]
    public bool? BlockPopUps { get; set; }

    /// <summary>
    /// Press Escape after the pop-up pass and again after scrolling (once in a video), for a modal that closes on
    /// Escape. Defaults to <see langword="false"/>.
    /// </summary>
    [JsonPropertyName("press_escape")]
    public bool? PressEscape { get; set; }

    /// <summary>How long the page may take to load, 1000 to 60000 milliseconds. Defaults to 30000.</summary>
    [JsonPropertyName("page_timeout_ms")]
    public int? PageTimeoutMs { get; set; }

    /// <summary>The pause after each scroll step of a full-page capture, 0 to 10000 milliseconds. Defaults to 500.</summary>
    [JsonPropertyName("scrolling_delay_ms")]
    public int? ScrollingDelayMs { get; set; }

    /// <summary>The browser's User-Agent, up to 512 characters.</summary>
    [JsonPropertyName("user_agent")]
    public string? UserAgent { get; set; }

    /// <summary>The browser locale as a BCP 47 tag, such as <c>en-GB</c>.</summary>
    [JsonPropertyName("locale")]
    public string? Locale { get; set; }

    /// <summary>The browser time zone as an IANA name, such as <c>Europe/London</c>.</summary>
    [JsonPropertyName("time_zone")]
    public string? TimeZone { get; set; }

    /// <summary>
    /// Headers sent with every request the page makes. Names are sent exactly as given. Needs the
    /// RequestHeadersCookies plan feature.
    /// </summary>
    [JsonPropertyName("headers")]
    public IDictionary<string, string>? Headers { get; set; }

    /// <summary>
    /// Cookies set for the URL's origin. Names are sent exactly as given. Needs the RequestHeadersCookies plan
    /// feature.
    /// </summary>
    [JsonPropertyName("cookies")]
    public IDictionary<string, string>? Cookies { get; set; }

    /// <summary>The position the browser reports.</summary>
    [JsonPropertyName("geolocation")]
    public Geolocation? Geolocation { get; set; }

    /// <summary>Elements to capture, hide, click or hover.</summary>
    [JsonPropertyName("selectors")]
    public Selectors? Selectors { get; set; }

    /// <summary>Load the page through a proxy in another country. Needs the ProxyLocation plan feature.</summary>
    [JsonPropertyName("proxy")]
    public ProxyOptions? Proxy { get; set; }

    /// <summary>Add a strip with the capture time and URL. Setting this turns it on. Not available for video.</summary>
    [JsonPropertyName("timestamp_watermark")]
    public TimestampWatermark? TimestampWatermark { get; set; }

    /// <summary>
    /// Also keep the page source. Charged as a second capture, capped at 10 MB, and not available with
    /// <see cref="InstantSnapClient.CaptureFileAsync"/>.
    /// </summary>
    [JsonPropertyName("page_source")]
    public PageSourceOptions? PageSource { get; set; }

    /// <summary>
    /// The bucket <see cref="InstantSnapClient.CaptureToBucketAsync"/> and
    /// <see cref="InstantSnapClient.SubmitToBucketAsync"/> upload to. Required there and refused by the other
    /// capture methods.
    /// </summary>
    [JsonPropertyName("s3_delivery")]
    public S3DeliveryOptions? S3Delivery { get; set; }

    /// <summary>
    /// A URL that also receives the <c>capture.succeeded</c> or <c>capture.failed</c> webhook. Needs the
    /// Webhooks plan feature.
    /// </summary>
    [JsonPropertyName("callback_url")]
    public string? CallbackUrl { get; set; }

    /// <summary>
    /// The browser time an async capture may use, 30 to 300 seconds; the API uses 120 when it is not set. Sent
    /// as <c>timeout</c>, for <see cref="InstantSnapClient.SubmitAsync"/> and
    /// <see cref="InstantSnapClient.SubmitToBucketAsync"/> only: the API refuses it on the other capture methods.
    /// Not to be confused with <see cref="RequestOptions.Timeout"/>, how long the client waits for an answer.
    /// </summary>
    [JsonPropertyName("timeout")]
    public int? TimeoutSeconds { get; set; }
}
