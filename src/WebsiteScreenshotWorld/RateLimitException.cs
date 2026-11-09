using System.Net;

namespace WebsiteScreenshotWorld;

/// <summary>
/// Too many requests. <c>rate_limited</c> is raised once the automatic retries are used up; <c>queue_full</c>
/// (too many async captures already waiting or running) is never retried, so wait <see cref="WebsiteScreenshotWorldException.RetryAfter"/>
/// before submitting again.
/// </summary>
/// <remarks>Error codes: <c>rate_limited</c>, <c>queue_full</c>.</remarks>
public class RateLimitException : WebsiteScreenshotWorldException
{
    /// <summary>Creates the error for an API refusal.</summary>
    /// <param name="message">The API's <c>error_message</c>.</param>
    /// <param name="errorCode">The API's <c>error_code</c>.</param>
    /// <param name="statusCode">The HTTP status of the answer.</param>
    /// <param name="retryAfter">How long the API asked the client to wait, if it did.</param>
    /// <param name="responseBody">The body of the answer as received.</param>
    public RateLimitException(
        string message,
        string errorCode,
        HttpStatusCode statusCode,
        TimeSpan? retryAfter = null,
        string? responseBody = null)
        : base(message, errorCode, statusCode, retryAfter, responseBody)
    {
    }
}
