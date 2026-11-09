using System.Net;

namespace WebsiteScreenshotWorld;

/// <summary>
/// The base of every error this library raises. An API refusal whose code this version does not know is raised
/// as this type; the known kinds are raised as its subclasses.
/// </summary>
/// <remarks>Decide what to do from <see cref="ErrorCode"/>, never from the status or the message.</remarks>
public class WebsiteScreenshotWorldException : Exception
{
    /// <summary>Creates an error with a message.</summary>
    /// <param name="message">What went wrong.</param>
    public WebsiteScreenshotWorldException(string message)
        : base(message)
    {
    }

    /// <summary>Creates an error with a message and the exception that caused it.</summary>
    /// <param name="message">What went wrong.</param>
    /// <param name="innerException">The cause.</param>
    public WebsiteScreenshotWorldException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }

    /// <summary>Creates an error for an API refusal.</summary>
    /// <param name="message">The API's <c>error_message</c>.</param>
    /// <param name="errorCode">The API's <c>error_code</c>.</param>
    /// <param name="statusCode">The HTTP status of the answer.</param>
    /// <param name="retryAfter">How long the API asked the client to wait, if it did.</param>
    /// <param name="responseBody">The body of the answer as received.</param>
    public WebsiteScreenshotWorldException(
        string message,
        string errorCode,
        HttpStatusCode statusCode,
        TimeSpan? retryAfter = null,
        string? responseBody = null)
        : base(message)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
        RetryAfter = retryAfter;
        ResponseBody = responseBody;
    }

    /// <summary>
    /// The API's error code, such as <c>rate_limited</c>. <see langword="null"/> when there was no answer.
    /// </summary>
    public string? ErrorCode { get; }

    /// <summary>The HTTP status of the answer. <see langword="null"/> when there was no answer.</summary>
    public HttpStatusCode? StatusCode { get; }

    /// <summary>How long the API asked the client to wait before trying again, if it did.</summary>
    public TimeSpan? RetryAfter { get; }

    /// <summary>The body of the answer as received, if there was one.</summary>
    public string? ResponseBody { get; }
}
