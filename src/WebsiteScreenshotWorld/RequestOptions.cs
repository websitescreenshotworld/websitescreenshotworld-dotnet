namespace WebsiteScreenshotWorld;

/// <summary>Settings for a single call.</summary>
public sealed class RequestOptions
{
    /// <summary>
    /// The <c>Idempotency-Key</c> for a capture, up to 255 printable ASCII characters. Sending the same key again
    /// within 24 hours returns the first capture instead of capturing twice. When not set, each capture call
    /// gets a new random key, reused on its retries. Other calls ignore it.
    /// </summary>
    public string? IdempotencyKey { get; set; }

    /// <summary>
    /// How long the whole call may take, retries included, replacing <see cref="WebsiteScreenshotWorldClientOptions.CaptureTimeout"/>
    /// or <see cref="WebsiteScreenshotWorldClientOptions.Timeout"/> for this call.
    /// </summary>
    public TimeSpan? Timeout { get; set; }
}
