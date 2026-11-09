namespace WebsiteScreenshotWorld;

/// <summary>Settings for <see cref="WebsiteScreenshotWorldClient"/>.</summary>
public sealed class WebsiteScreenshotWorldClientOptions
{
    /// <summary>The environment variable the Instant Snap key is read from when none is set.</summary>
    public const string InstantSnapApiKeyVariable = "WSW_INSTANT_SNAP_API_KEY";

    /// <summary>The environment variable the Auto Schedule key is read from when none is set.</summary>
    public const string AutoScheduleApiKeyVariable = "WSW_AUTO_SCHEDULE_API_KEY";

    /// <summary>
    /// The Instant Snap API key. When not set, it is read from <c>WSW_INSTANT_SNAP_API_KEY</c> as the client is
    /// created. Instant Snap calls other than defaults fail before sending anything when there is no key.
    /// </summary>
    public string? InstantSnapApiKey { get; set; }

    /// <summary>
    /// The Auto Schedule API key. When not set, it is read from <c>WSW_AUTO_SCHEDULE_API_KEY</c> as the client is
    /// created. Auto Schedule calls fail before sending anything when there is no key.
    /// </summary>
    public string? AutoScheduleApiKey { get; set; }

    /// <summary>The Instant Snap API address.</summary>
    public Uri InstantSnapBaseUrl { get; set; } = new("https://api.websitescreenshotworld.com");

    /// <summary>The Auto Schedule API address.</summary>
    public Uri AutoScheduleBaseUrl { get; set; } = new("https://autoapi.websitescreenshotworld.com");

    /// <summary>
    /// How long a capture call may take, retries and the waits between them included. Defaults to 180 seconds, since
    /// the API may spend up to 120 seconds on a capture.
    /// </summary>
    public TimeSpan CaptureTimeout { get; set; } = TimeSpan.FromSeconds(180);

    /// <summary>How long any other call may take, retries included. Defaults to 30 seconds.</summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// How many times a call is retried after a connection failure or a retryable refusal. Defaults to 2, so a
    /// call makes at most 3 attempts. Set 0 to turn retries off.
    /// </summary>
    public int MaxRetries { get; set; } = 2;
}
