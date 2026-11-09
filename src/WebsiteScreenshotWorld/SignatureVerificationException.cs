namespace WebsiteScreenshotWorld;

/// <summary>
/// A webhook's signature header is missing or malformed, its timestamp is outside the tolerance, or no signature
/// matches the body. Answer such a request with 400 and do not act on it.
/// </summary>
public class SignatureVerificationException : WebsiteScreenshotWorldException
{
    /// <summary>Creates the error.</summary>
    /// <param name="message">What was wrong with the signature.</param>
    public SignatureVerificationException(string message)
        : base(message)
    {
    }
}
