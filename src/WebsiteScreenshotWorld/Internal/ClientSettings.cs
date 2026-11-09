namespace WebsiteScreenshotWorld.Internal;

/// <summary>A validated copy of the client options, taken when the client is created.</summary>
internal sealed class ClientSettings
{
    private ClientSettings(WebsiteScreenshotWorldClientOptions options)
    {
        InstantSnapApiKey = KeyOrEnvironment(options.InstantSnapApiKey, WebsiteScreenshotWorldClientOptions.InstantSnapApiKeyVariable);
        AutoScheduleApiKey = KeyOrEnvironment(options.AutoScheduleApiKey, WebsiteScreenshotWorldClientOptions.AutoScheduleApiKeyVariable);
        InstantSnapBaseUrl = BaseUrl(options.InstantSnapBaseUrl, nameof(options.InstantSnapBaseUrl));
        AutoScheduleBaseUrl = BaseUrl(options.AutoScheduleBaseUrl, nameof(options.AutoScheduleBaseUrl));
        CaptureTimeout = Positive(options.CaptureTimeout, nameof(options.CaptureTimeout));
        Timeout = Positive(options.Timeout, nameof(options.Timeout));
        MaxRetries = options.MaxRetries >= 0
            ? options.MaxRetries
            : throw new ArgumentOutOfRangeException(nameof(options), $"{nameof(options.MaxRetries)} cannot be negative.");
    }

    public string? InstantSnapApiKey { get; }

    public string? AutoScheduleApiKey { get; }

    public string InstantSnapBaseUrl { get; }

    public string AutoScheduleBaseUrl { get; }

    public TimeSpan CaptureTimeout { get; }

    public TimeSpan Timeout { get; }

    public int MaxRetries { get; }

    public static ClientSettings From(WebsiteScreenshotWorldClientOptions? options) =>
        new(options ?? new WebsiteScreenshotWorldClientOptions());

    public string RequireInstantSnapKey() =>
        InstantSnapApiKey ?? throw MissingKey("Instant Snap", nameof(WebsiteScreenshotWorldClientOptions.InstantSnapApiKey), WebsiteScreenshotWorldClientOptions.InstantSnapApiKeyVariable);

    public string RequireAutoScheduleKey() =>
        AutoScheduleApiKey ?? throw MissingKey("Auto Schedule", nameof(WebsiteScreenshotWorldClientOptions.AutoScheduleApiKey), WebsiteScreenshotWorldClientOptions.AutoScheduleApiKeyVariable);

    public TimeSpan TimeoutFor(RequestOptions? requestOptions, bool isCapture)
    {
        if (requestOptions?.Timeout is { } timeout)
        {
            return Positive(timeout, nameof(requestOptions.Timeout));
        }

        return isCapture ? CaptureTimeout : Timeout;
    }

    private static string? KeyOrEnvironment(string? key, string variable)
    {
        var value = string.IsNullOrWhiteSpace(key) ? Environment.GetEnvironmentVariable(variable) : key;
        return string.IsNullOrWhiteSpace(value) ? null : value!.Trim();
    }

    private static string BaseUrl(Uri? url, string name)
    {
        if (url is null || !url.IsAbsoluteUri)
        {
            throw new ArgumentException($"{name} must be an absolute URL.", name);
        }

        return url.AbsoluteUri.TrimEnd('/');
    }

    private static TimeSpan Positive(TimeSpan value, string name)
    {
        if (value <= TimeSpan.Zero && value != System.Threading.Timeout.InfiniteTimeSpan)
        {
            throw new ArgumentOutOfRangeException(name, value, "A timeout must be positive.");
        }

        return value;
    }

    private static InvalidOperationException MissingKey(string product, string option, string variable) =>
        new($"No {product} API key is set. Set {nameof(WebsiteScreenshotWorldClientOptions)}.{option} or the {variable} environment variable.");
}
