using System.Net;

namespace WebsiteScreenshotWorld;

/// <summary>The request was not valid and will not succeed if sent again unchanged.</summary>
/// <remarks>Error codes: <c>validation_failed</c>, <c>s3_destination_invalid</c>, <c>web_page_id_invalid</c>, <c>idempotency_key_reused</c>, <c>sitemap_schedule_not_triggerable</c>.</remarks>
public class InvalidRequestException : WebsiteScreenshotWorldException
{
    /// <summary>Creates the error for an API refusal.</summary>
    /// <param name="message">The API's <c>error_message</c>.</param>
    /// <param name="errorCode">The API's <c>error_code</c>.</param>
    /// <param name="statusCode">The HTTP status of the answer.</param>
    /// <param name="retryAfter">How long the API asked the client to wait, if it did.</param>
    /// <param name="responseBody">The body of the answer as received.</param>
    public InvalidRequestException(
        string message,
        string errorCode,
        HttpStatusCode statusCode,
        TimeSpan? retryAfter = null,
        string? responseBody = null)
        : base(message, errorCode, statusCode, retryAfter, responseBody)
    {
    }
}
