namespace WebsiteScreenshotWorld.Internal;

/// <summary>Cancellable content reads on every target framework.</summary>
internal static class HttpContentCompat
{
    public static Task<Stream> ReadAsStreamCompatAsync(this HttpContent content, CancellationToken cancellationToken)
    {
#if NET
        return content.ReadAsStreamAsync(cancellationToken);
#else
        cancellationToken.ThrowIfCancellationRequested();
        return content.ReadAsStreamAsync();
#endif
    }

    public static Task<string> ReadAsStringCompatAsync(this HttpContent content, CancellationToken cancellationToken)
    {
#if NET
        return content.ReadAsStringAsync(cancellationToken);
#else
        cancellationToken.ThrowIfCancellationRequested();
        return content.ReadAsStringAsync();
#endif
    }
}
