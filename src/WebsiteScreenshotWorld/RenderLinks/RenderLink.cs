using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using WebsiteScreenshotWorld.InstantSnap;
using WebsiteScreenshotWorld.Internal;

namespace WebsiteScreenshotWorld.RenderLinks;

/// <summary>Render links: signed URLs of an Instant Snap key that are themselves a screenshot.</summary>
public static class RenderLink
{
    private const string PathPrefix = "/instant-snap/v1/render/";
    private const string DefaultBaseUrl = "https://api.websitescreenshotworld.com";

    /// <summary>
    /// Builds a render link: a URL that is itself a screenshot, for an <c>&lt;img&gt;</c> tag, an email or a
    /// social preview. It is made here, locally - nothing is sent, and no client or API key is needed. Every view
    /// of the link that is not already cached makes one Instant Snap capture.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// The render secret, key prefix or URL is missing, the key prefix is not 12 lower-case hex characters, or
    /// the format is not png, jpeg or webp.
    /// </exception>
    public static string Build(RenderLinkOptions options)
    {
        Guard.NotNull(options, nameof(options));

        if (string.IsNullOrEmpty(options.RenderSecret))
        {
            throw new ArgumentException("The render secret is required.", nameof(options));
        }

        if (!IsKeyPrefix(options.KeyPrefix))
        {
            throw new ArgumentException("The key prefix must be the key's 12-character prefix (lower-case hex).", nameof(options));
        }

        if (string.IsNullOrEmpty(options.Url))
        {
            throw new ArgumentException("The URL is required.", nameof(options));
        }

        var format = (options.Format ?? CaptureFormat.Png).ToString();
        if (format != "png" && format != "jpeg" && format != "webp")
        {
            throw new ArgumentException("The format must be png, jpeg or webp.", nameof(options));
        }

        var pairs = new List<KeyValuePair<string, string>>();
        void Add(string name, string? value)
        {
            if (value is not null)
            {
                pairs.Add(new KeyValuePair<string, string>(Encode(name), Encode(value)));
            }
        }

        Add("url", options.Url);
        Add("ttl", Number(options.TtlSeconds));
        Add("width", Number(options.Width));
        Add("height", Number(options.Height));
        Add("device_scale_factor", options.DeviceScaleFactor?.ToString("R", CultureInfo.InvariantCulture));
        Add("device_category", options.DeviceCategory?.ToString());
        Add("emulated_device", options.EmulatedDevice);
        Add("scroll_mode", options.ScrollMode?.ToString());
        Add("auto_height", Boolean(options.AutoHeight));
        Add("max_height", Number(options.MaxHeight));
        Add("zoom", Number(options.Zoom));
        Add("block_ads", Boolean(options.BlockAds));
        Add("block_pop_ups", Boolean(options.BlockPopUps));
        Add("press_escape", Boolean(options.PressEscape));
        Add("page_timeout_ms", Number(options.PageTimeoutMs));
        Add("scrolling_delay_ms", Number(options.ScrollingDelayMs));
        Add("locale", options.Locale);
        Add("time_zone", options.TimeZone);
        Add("hide", options.Hide);
        Add("xpath", options.XPath);
        Add("click", options.Click);
        Add("hover", options.Hover);
        Add("country", options.Country);
        Add("thumb_width", Number(options.ThumbWidth));

        pairs.Sort((left, right) => string.CompareOrdinal(left.Key, right.Key));

        var canonical = new StringBuilder(PathPrefix).Append(options.KeyPrefix).Append('/').Append(format).Append('?');
        for (var index = 0; index < pairs.Count; index++)
        {
            if (index > 0)
            {
                canonical.Append('&');
            }

            canonical.Append(pairs[index].Key).Append('=').Append(pairs[index].Value);
        }

        var canonicalText = canonical.ToString();
        var baseUrl = (options.BaseUrl?.ToString() ?? DefaultBaseUrl).TrimEnd('/');

        return baseUrl + canonicalText + "&sig=" + Sign(options.RenderSecret!, canonicalText);
    }

    private static string? Number(int? value) => value?.ToString(CultureInfo.InvariantCulture);

    private static string? Boolean(bool? value) => value is null ? null : value.Value ? "true" : "false";

    private static bool IsKeyPrefix(string? value)
    {
        if (value is null || value.Length != 12)
        {
            return false;
        }

        foreach (var character in value)
        {
            if (character is not (>= '0' and <= '9' or >= 'a' and <= 'f'))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>RFC 3986: every UTF-8 byte but <c>A-Z a-z 0-9 - . _ ~</c>, upper-case hex.</summary>
    private static string Encode(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var b in Encoding.UTF8.GetBytes(value))
        {
            var c = (char)b;
            if (c is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9' or '-' or '.' or '_' or '~')
            {
                builder.Append(c);
            }
            else
            {
                builder.Append('%').Append(b.ToString("X2", CultureInfo.InvariantCulture));
            }
        }

        return builder.ToString();
    }

    private static string Sign(string secret, string canonical)
    {
        var key = Encoding.UTF8.GetBytes(secret);
        var message = Encoding.UTF8.GetBytes(canonical);
#if NET
        var mac = HMACSHA256.HashData(key, message);
#else
        using var hmac = new HMACSHA256(key);
        var mac = hmac.ComputeHash(message);
#endif
        var hex = new StringBuilder(mac.Length * 2);
        foreach (var b in mac)
        {
            hex.Append(b.ToString("x2", CultureInfo.InvariantCulture));
        }

        return hex.ToString();
    }
}
