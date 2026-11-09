using WebsiteScreenshotWorld.AutoSchedule;
using WebsiteScreenshotWorld.InstantSnap;
using WebsiteScreenshotWorld.Internal;

namespace WebsiteScreenshotWorld;

/// <summary>
/// The entry point to the Website Screenshot World APIs. Create one and reuse it; it is safe to share between
/// threads.
/// </summary>
/// <example>
/// <code>
/// using var client = new WebsiteScreenshotWorldClient();
/// var capture = await client.InstantSnap.CaptureAsync(new CaptureOptions { Url = "https://example.com" });
/// </code>
/// </example>
public sealed class WebsiteScreenshotWorldClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly bool _ownsHttpClient;

    /// <summary>Creates a client with its own <see cref="HttpClient"/>.</summary>
    /// <param name="options">Keys and settings. When <see langword="null"/>, keys come from the environment.</param>
    public WebsiteScreenshotWorldClient(WebsiteScreenshotWorldClientOptions? options = null)
        : this(ClientSettings.From(options), CreateHttpClient(), ownsHttpClient: true)
    {
    }

    /// <summary>
    /// Creates a client that sends through <paramref name="httpClient"/>, for example one from
    /// <c>IHttpClientFactory</c>. The client does not dispose it.
    /// </summary>
    /// <remarks>
    /// This client applies its own timeouts, so set <see cref="HttpClient.Timeout"/> to
    /// <see cref="Timeout.InfiniteTimeSpan"/> or to more than <see cref="WebsiteScreenshotWorldClientOptions.CaptureTimeout"/>.
    /// The default of 100 seconds would cut long captures short. Its handler must not follow redirects
    /// (<c>AllowAutoRedirect = false</c>): a redirect followed would carry the API key to another host, and an answer
    /// that came through one is refused.
    /// </remarks>
    /// <param name="httpClient">The client to send requests through.</param>
    /// <param name="options">Keys and settings. When <see langword="null"/>, keys come from the environment.</param>
    public WebsiteScreenshotWorldClient(HttpClient httpClient, WebsiteScreenshotWorldClientOptions? options = null)
        : this(ClientSettings.From(options), httpClient ?? throw new ArgumentNullException(nameof(httpClient)), ownsHttpClient: false)
    {
    }

    private WebsiteScreenshotWorldClient(ClientSettings settings, HttpClient httpClient, bool ownsHttpClient)
    {
        _httpClient = httpClient;
        _ownsHttpClient = ownsHttpClient;

        var transport = new ApiTransport(httpClient, settings.MaxRetries);
        InstantSnap = new InstantSnapClient(transport, settings);
        AutoSchedule = new AutoScheduleClient(transport, settings);
    }

    /// <summary>On-demand captures: images, videos and PDFs.</summary>
    public InstantSnapClient InstantSnap { get; }

    /// <summary>Captures of web pages scheduled in the dashboard.</summary>
    public AutoScheduleClient AutoSchedule { get; }

    /// <summary>Disposes the <see cref="HttpClient"/> if this client created it.</summary>
    public void Dispose()
    {
        if (_ownsHttpClient)
        {
            _httpClient.Dispose();
        }
    }

    private static HttpClient CreateHttpClient()
    {
        // Redirects are never followed: the API key would go wherever one pointed. Neither the API nor a download
        // link redirects, so a 3xx is raised as the refusal it is.
#if NET
        // Recycles pooled connections so a long-lived client notices DNS changes.
        var handler = new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
            AllowAutoRedirect = false,
        };
#else
        var handler = new HttpClientHandler { AllowAutoRedirect = false };
#endif
        return new HttpClient(handler) { Timeout = Timeout.InfiniteTimeSpan };
    }
}
