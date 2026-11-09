using System.Net;

namespace WebsiteScreenshotWorld;

/// <summary>The service could not handle the request. Busy and unavailable answers are retried first.</summary>
/// <remarks>Error codes: <c>capture_busy</c>, <c>service_unavailable</c>, <c>internal_error</c>, <c>request_cancelled</c>, <c>idempotency_key_in_use</c>.</remarks>
public class ServiceException : WebsiteScreenshotWorldException
{
    /// <summary>Creates the error for an API refusal.</summary>
    /// <param name="message">The API's <c>error_message</c>.</param>
    /// <param name="errorCode">The API's <c>error_code</c>.</param>
    /// <param name="statusCode">The HTTP status of the answer.</param>
    /// <param name="retryAfter">How long the API asked the client to wait, if it did.</param>
    /// <param name="responseBody">The body of the answer as received.</param>
    public ServiceException(
        string message,
        string errorCode,
        HttpStatusCode statusCode,
        TimeSpan? retryAfter = null,
        string? responseBody = null)
        : base(message, errorCode, statusCode, retryAfter, responseBody)
    {
    }
}
