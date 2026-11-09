namespace WebsiteScreenshotWorld;

/// <summary>
/// No answer came back: the request could not be sent, the connection broke, or the timeout passed. Raised once
/// the automatic retries are used up. <see cref="Exception.InnerException"/> holds the cause, a
/// <see cref="TimeoutException"/> when the timeout passed.
/// </summary>
public class WebsiteScreenshotWorldConnectionException : WebsiteScreenshotWorldException
{
    /// <summary>Creates the error.</summary>
    /// <param name="message">What went wrong.</param>
    /// <param name="innerException">The cause.</param>
    public WebsiteScreenshotWorldConnectionException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
