using System.Text.Json;
using WebsiteScreenshotWorld.InstantSnap;
using WebsiteScreenshotWorld.RenderLinks;
using WebsiteScreenshotWorld.Tests.Support;

namespace WebsiteScreenshotWorld.Tests;

/// <summary>Runs every case in <c>spec/render-link-vectors.json</c>.</summary>
public sealed class RenderLinkVectorTests
{
    private static readonly Lazy<JsonDocument> Vectors =
        new(() => JsonDocument.Parse(File.ReadAllText(RepoPaths.Spec("render-link-vectors.json"))));

    public static TheoryData<string> Cases() =>
        new(Vectors.Value.RootElement.GetProperty("cases").EnumerateArray().Select(c => c.GetProperty("name").GetString()!));

    [Theory]
    [MemberData(nameof(Cases))]
    public void BuildsTheVectorsLink(string name)
    {
        var vector = Vectors.Value.RootElement.GetProperty("cases").EnumerateArray()
            .Single(c => c.GetProperty("name").GetString() == name);

        var options = new RenderLinkOptions
        {
            RenderSecret = vector.GetProperty("render_secret").GetString(),
            KeyPrefix = vector.GetProperty("key_prefix").GetString(),
            Format = vector.TryGetProperty("format", out var format) ? new CaptureFormat(format.GetString()!) : (CaptureFormat?)null,
            BaseUrl = vector.TryGetProperty("base_url", out var baseUrl) ? new Uri(baseUrl.GetString()!) : null
        };

        foreach (var option in vector.GetProperty("options").EnumerateObject())
        {
            Apply(options, option.Name, option.Value);
        }

        Assert.Equal(vector.GetProperty("url").GetString(), RenderLink.Build(options));
    }

    [Fact]
    public void RefusesWhatCannotBeALink()
    {
        static RenderLinkOptions Valid() => new() { RenderSecret = "wsw_rsk_x", KeyPrefix = "0123456789ab", Url = "https://example.com" };

        Assert.Matches("&sig=[0-9a-f]{64}$", RenderLink.Build(Valid()));
        Assert.Throws<ArgumentException>(() => RenderLink.Build(new RenderLinkOptions { KeyPrefix = "0123456789ab", Url = "https://example.com" }));
        var upper = Valid();
        upper.KeyPrefix = "0123456789AB";
        Assert.Throws<ArgumentException>(() => RenderLink.Build(upper));
        var noUrl = Valid();
        noUrl.Url = null;
        Assert.Throws<ArgumentException>(() => RenderLink.Build(noUrl));
        var pdf = Valid();
        pdf.Format = new CaptureFormat("pdf");
        Assert.Throws<ArgumentException>(() => RenderLink.Build(pdf));
    }

    private static void Apply(RenderLinkOptions options, string name, JsonElement value)
    {
        switch (name)
        {
            case "url": options.Url = value.GetString(); break;
            case "ttl": options.TtlSeconds = value.GetInt32(); break;
            case "width": options.Width = value.GetInt32(); break;
            case "height": options.Height = value.GetInt32(); break;
            case "device_scale_factor": options.DeviceScaleFactor = value.GetDouble(); break;
            case "device_category": options.DeviceCategory = new DeviceCategory(value.GetString()!); break;
            case "emulated_device": options.EmulatedDevice = value.GetString(); break;
            case "scroll_mode": options.ScrollMode = new ScrollMode(value.GetString()!); break;
            case "auto_height": options.AutoHeight = value.GetBoolean(); break;
            case "max_height": options.MaxHeight = value.GetInt32(); break;
            case "zoom": options.Zoom = value.GetInt32(); break;
            case "block_ads": options.BlockAds = value.GetBoolean(); break;
            case "block_pop_ups": options.BlockPopUps = value.GetBoolean(); break;
            case "press_escape": options.PressEscape = value.GetBoolean(); break;
            case "page_timeout_ms": options.PageTimeoutMs = value.GetInt32(); break;
            case "scrolling_delay_ms": options.ScrollingDelayMs = value.GetInt32(); break;
            case "locale": options.Locale = value.GetString(); break;
            case "time_zone": options.TimeZone = value.GetString(); break;
            case "hide": options.Hide = value.GetString(); break;
            case "xpath": options.XPath = value.GetString(); break;
            case "click": options.Click = value.GetString(); break;
            case "hover": options.Hover = value.GetString(); break;
            case "country": options.Country = value.GetString(); break;
            case "thumb_width": options.ThumbWidth = value.GetInt32(); break;
            default: throw new InvalidOperationException($"The vector names an option this test does not know: {name}");
        }
    }
}
