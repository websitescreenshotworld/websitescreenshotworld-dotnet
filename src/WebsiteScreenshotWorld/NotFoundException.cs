using System.Net;

namespace WebsiteScreenshotWorld;

/// <summary>The capture or web page does not exist for this account.</summary>
/// <remarks>Error codes: <c>capture_not_found</c>, <c>web_page_not_found</c>, <c>not_found</c>.</remarks>
public class NotFoundException : WebsiteScreenshotWorldException
{
    /// <summary>Creates the error for an API refusal.</summary>
    /// <param name="message">The API's <c>error_message</c>.</param>
    /// <param name="errorCode">The API's <c>error_code</c>.</param>
    /// <param name="statusCode">The HTTP status of the answer.</param>
    /// <param name="retryAfter">How long the API asked the client to wait, if it did.</param>
    /// <param name="responseBody">The body of the answer as received.</param>
    public NotFoundException(
        string message,
        string errorCode,
        HttpStatusCode statusCode,
        TimeSpan? retryAfter = null,
        string? responseBody = null)
        : base(message, errorCode, statusCode, retryAfter, responseBody)
    {
    }
}
