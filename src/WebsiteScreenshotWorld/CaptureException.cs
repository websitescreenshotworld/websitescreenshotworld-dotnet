using System.Net;

namespace WebsiteScreenshotWorld;

/// <summary>The website could not be captured, or the file was too large to return. Not retried.</summary>
/// <remarks>
/// Error codes: <c>capture_failed</c>, <c>capture_timeout</c>, <c>payload_too_large</c>, and
/// <c>capture_expired</c> (an async capture still waiting 15 minutes after it was accepted; not charged).
/// </remarks>
public class CaptureException : WebsiteScreenshotWorldException
{
    /// <summary>Creates the error for an API refusal.</summary>
    /// <param name="message">The API's <c>error_message</c>.</param>
    /// <param name="errorCode">The API's <c>error_code</c>.</param>
    /// <param name="statusCode">The HTTP status of the answer.</param>
    /// <param name="retryAfter">How long the API asked the client to wait, if it did.</param>
    /// <param name="responseBody">The body of the answer as received.</param>
    public CaptureException(
        string message,
        string errorCode,
        HttpStatusCode statusCode,
        TimeSpan? retryAfter = null,
        string? responseBody = null)
        : base(message, errorCode, statusCode, retryAfter, responseBody)
    {
    }
}
