using System.Net;

namespace WebsiteScreenshotWorld;

/// <summary>The plan has no allowance left, or does not include what was asked for.</summary>
/// <remarks>Error codes: <c>quota_exhausted</c>, <c>feature_not_available</c>, <c>no_trigger_allowance</c>, <c>forbidden</c>.</remarks>
public class PlanLimitException : WebsiteScreenshotWorldException
{
    /// <summary>Creates the error for an API refusal.</summary>
    /// <param name="message">The API's <c>error_message</c>.</param>
    /// <param name="errorCode">The API's <c>error_code</c>.</param>
    /// <param name="statusCode">The HTTP status of the answer.</param>
    /// <param name="retryAfter">How long the API asked the client to wait, if it did.</param>
    /// <param name="responseBody">The body of the answer as received.</param>
    public PlanLimitException(
        string message,
        string errorCode,
        HttpStatusCode statusCode,
        TimeSpan? retryAfter = null,
        string? responseBody = null)
        : base(message, errorCode, statusCode, retryAfter, responseBody)
    {
    }
}
