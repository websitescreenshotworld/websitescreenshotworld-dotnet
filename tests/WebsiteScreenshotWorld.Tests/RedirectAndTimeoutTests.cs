using System.Diagnostics;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using WebsiteScreenshotWorld.Tests.Support;

namespace WebsiteScreenshotWorld.Tests;

public sealed class RedirectAndTimeoutTests
{
    private static readonly Guid WebPageId = Guid.Parse("7d4e2b1a-6c5f-4a3e-9d8c-2b1a0f9e8d7c");

    [Fact]
    public async Task OwnHttpClientDoesNotFollowRedirects()
    {
        using var server = new RedirectServer();
        using var client = new WebsiteScreenshotWorldClient(new WebsiteScreenshotWorldClientOptions
        {
            AutoScheduleApiKey = "wsw_autoapi_secret",
            AutoScheduleBaseUrl = server.BaseUrl,
        });

        var exception = await Assert.ThrowsAnyAsync<WebsiteScreenshotWorldException>(() => client.AutoSchedule.TriggerNowAsync(WebPageId));

        Assert.Equal(HttpStatusCode.Found, exception.StatusCode);
        Assert.Equal(0, server.Elsewhere);
    }

    [Fact]
    public async Task DependencyInjectionClientDoesNotFollowRedirects()
    {
        using var server = new RedirectServer();
        var services = new ServiceCollection();
        services.AddWebsiteScreenshotWorld(options =>
        {
            options.AutoScheduleApiKey = "wsw_autoapi_secret";
            options.AutoScheduleBaseUrl = server.BaseUrl;
        });
        using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<WebsiteScreenshotWorldClient>();

        var exception = await Assert.ThrowsAnyAsync<WebsiteScreenshotWorldException>(() => client.AutoSchedule.TriggerNowAsync(WebPageId));

        Assert.Equal(HttpStatusCode.Found, exception.StatusCode);
        Assert.Equal(0, server.Elsewhere);
    }

    [Fact]
    public async Task AnswerThroughARedirectIsRefused()
    {
        // An HttpClient passed in whose handler follows redirects: the key has gone, but the answer is not used.
        using var server = new RedirectServer();
        using var client = new WebsiteScreenshotWorldClient(new HttpClient(), new WebsiteScreenshotWorldClientOptions
        {
            AutoScheduleApiKey = "wsw_autoapi_secret",
            AutoScheduleBaseUrl = server.BaseUrl,
        });

        var exception = await Assert.ThrowsAsync<WebsiteScreenshotWorldException>(() => client.AutoSchedule.TriggerNowAsync(WebPageId));

        Assert.Contains("redirect", exception.Message, StringComparison.Ordinal);
        Assert.Contains("AllowAutoRedirect", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TimedOutCallIsNotRetried()
    {
        var handler = new StubHandler(async (_, token) =>
        {
            await Task.Delay(Timeout.Infinite, token);
            throw new InvalidOperationException("Unreachable.");
        });
        using var client = Client(handler, maxRetries: 2);

        var exception = await Assert.ThrowsAsync<WebsiteScreenshotWorldConnectionException>(
            () => client.AutoSchedule.TriggerNowAsync(WebPageId, new RequestOptions { Timeout = TimeSpan.FromMilliseconds(100) }));

        Assert.IsType<TimeoutException>(exception.InnerException);
        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task TimeoutCoversRetries()
    {
        var handler = new StubHandler((_, _) => Task.FromResult(StubHandler.JsonResponse(
            """{"success":false,"error_code":"service_unavailable","error_message":"Down.","retry_after_seconds":0.05}""",
            HttpStatusCode.ServiceUnavailable)));
        using var client = Client(handler, maxRetries: 1000);
        var stopwatch = Stopwatch.StartNew();

        await Assert.ThrowsAnyAsync<WebsiteScreenshotWorldException>(
            () => client.AutoSchedule.TriggerNowAsync(WebPageId, new RequestOptions { Timeout = TimeSpan.FromMilliseconds(300) }));

        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(2), $"Took {stopwatch.Elapsed}.");
        Assert.InRange(handler.Requests.Count, 2, 10);
    }

    [Fact]
    public async Task RetryWaitBeyondTheTimeoutIsNotTaken()
    {
        var handler = new StubHandler((_, _) => Task.FromResult(StubHandler.JsonResponse(
            """{"success":false,"error_code":"rate_limited","error_message":"Slow down.","retry_after_seconds":30}""",
            HttpStatusCode.TooManyRequests)));
        using var client = Client(handler);
        var stopwatch = Stopwatch.StartNew();

        var exception = await Assert.ThrowsAsync<RateLimitException>(
            () => client.AutoSchedule.TriggerNowAsync(WebPageId, new RequestOptions { Timeout = TimeSpan.FromSeconds(2) }));

        Assert.Equal(TimeSpan.FromSeconds(30), exception.RetryAfter);
        Assert.Single(handler.Requests);
        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(1), $"Took {stopwatch.Elapsed}.");
    }

    private static WebsiteScreenshotWorldClient Client(StubHandler handler, int maxRetries = 2) =>
        new(new HttpClient(handler), new WebsiteScreenshotWorldClientOptions
        {
            AutoScheduleApiKey = "wsw_autoapi_test",
            MaxRetries = maxRetries,
        });
}
