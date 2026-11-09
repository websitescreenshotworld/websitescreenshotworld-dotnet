using Microsoft.Extensions.Options;
using WebsiteScreenshotWorld;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>Registers <see cref="WebsiteScreenshotWorldClient"/> with a service collection.</summary>
public static class WebsiteScreenshotWorldServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="WebsiteScreenshotWorldClient"/> as a typed client whose <see cref="HttpClient"/> comes
    /// from <c>IHttpClientFactory</c>. Keys not set in <paramref name="configure"/> or bound options are read from
    /// the environment.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Sets keys and other settings. Options bound from configuration apply as well.</param>
    /// <returns>The builder for the underlying <see cref="HttpClient"/>, to add handlers or change its settings.</returns>
    /// <example>
    /// <code>
    /// builder.Services.AddWebsiteScreenshotWorld(options =>
    ///     options.InstantSnapApiKey = builder.Configuration["WebsiteScreenshotWorld:InstantSnapApiKey"]);
    /// </code>
    /// </example>
    public static IHttpClientBuilder AddWebsiteScreenshotWorld(
        this IServiceCollection services,
        Action<WebsiteScreenshotWorldClientOptions>? configure = null)
    {
        _ = services ?? throw new ArgumentNullException(nameof(services));

        var options = services.AddOptions<WebsiteScreenshotWorldClientOptions>();
        if (configure is not null)
        {
            options.Configure(configure);
        }

        return services
            .AddHttpClient(nameof(WebsiteScreenshotWorldClient))
            // The client applies its own timeouts, which are longer than HttpClient's default.
            .ConfigureHttpClient(httpClient => httpClient.Timeout = Timeout.InfiniteTimeSpan)
            // Redirects are never followed: the API key would go wherever one pointed. A primary handler set later
            // on the returned builder must keep AllowAutoRedirect off too, or its answers are refused.
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false })
            .AddTypedClient((httpClient, provider) => new WebsiteScreenshotWorldClient(
                httpClient,
                provider.GetRequiredService<IOptions<WebsiteScreenshotWorldClientOptions>>().Value));
    }
}
