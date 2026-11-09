using System.Diagnostics;
using System.Net;
using WebsiteScreenshotWorld.Tests.Support;

namespace WebsiteScreenshotWorld.Tests;

public sealed class RetryAndErrorTests
{
    private static readonly Guid WebPageId = Guid.Parse("7d4e2b1a-6c5f-4a3e-9d8c-2b1a0f9e8d7c");

    [Fact]
    public async Task ConnectionFailuresAreRetriedThenRaised()
    {
        var handler = new StubHandler((_, _) => throw new HttpRequestException("Connection refused"));
        using var client = Client(handler);

        var exception = await Assert.ThrowsAsync<WebsiteScreenshotWorldConnectionException>(() => client.AutoSchedule.TriggerNowAsync(WebPageId));

        Assert.Equal(3, handler.Requests.Count);
        Assert.IsType<HttpRequestException>(exception.InnerException);
        Assert.Null(exception.ErrorCode);
    }

    [Fact]
    public async Task TimeoutIsRaisedAsConnectionErrorWrappingTimeoutException()
    {
        var handler = new StubHandler(async (_, token) =>
        {
            await Task.Delay(Timeout.Infinite, token);
            throw new InvalidOperationException("Unreachable.");
        });
        using var client = Client(handler, maxRetries: 0);

        var exception = await Assert.ThrowsAsync<WebsiteScreenshotWorldConnectionException>(
            () => client.AutoSchedule.TriggerNowAsync(WebPageId, new RequestOptions { Timeout = TimeSpan.FromMilliseconds(50) }));

        Assert.IsType<TimeoutException>(exception.InnerException);
    }

    [Fact]
    public async Task CancellationStopsAtOnce()
    {
        // A wait that fits in the call's 30-second timeout, so it is the cancellation that ends it.
        var handler = new StubHandler((_, _) => Task.FromResult(StubHandler.JsonResponse(
            """{"success":false,"error_code":"service_unavailable","error_message":"Down.","retry_after_seconds":5}""",
            HttpStatusCode.ServiceUnavailable)));
        using var client = Client(handler);
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));
        var stopwatch = Stopwatch.StartNew();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => client.AutoSchedule.TriggerNowAsync(WebPageId, cancellationToken: cancellation.Token));

        Assert.Single(handler.Requests);
        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(10));
    }

    [Fact]
    public async Task RetryAfterHeaderIsUsedWhenBodyHasNone()
    {
        var handler = new StubHandler((_, _) =>
        {
            var response = StubHandler.JsonResponse(
                """{"success":false,"error_code":"rate_limited","error_message":"Slow down.","retry_after_seconds":null}""",
                HttpStatusCode.TooManyRequests);
            response.Headers.Add("Retry-After", "7");
            return Task.FromResult(response);
        });
        using var client = Client(handler, maxRetries: 0);

        var exception = await Assert.ThrowsAsync<RateLimitException>(() => client.AutoSchedule.TriggerNowAsync(WebPageId));

        Assert.Equal(TimeSpan.FromSeconds(7), exception.RetryAfter);
        Assert.Equal(HttpStatusCode.TooManyRequests, exception.StatusCode);
        Assert.Contains("rate_limited", exception.ResponseBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TriggerNowSendsNoBody()
    {
        var handler = StubHandler.Json(
            """{"success":true,"status":"triggered","web_page_id":"7d4e2b1a-6c5f-4a3e-9d8c-2b1a0f9e8d7c","triggered_by":"api","triggered_at":"2026-09-26T10:30:00.000Z","message":"Queued."}""");
        using var client = Client(handler);

        var result = await client.AutoSchedule.TriggerNowAsync(WebPageId);

        Assert.Null(handler.Requests.Single().Body);
        Assert.Equal("application/json", handler.Requests.Single().Headers["Accept"]);
        Assert.Equal(new DateTimeOffset(2026, 9, 26, 10, 30, 0, TimeSpan.Zero), result.TriggeredAt);
    }

    private static WebsiteScreenshotWorldClient Client(StubHandler handler, int maxRetries = 2) =>
        new(new HttpClient(handler), new WebsiteScreenshotWorldClientOptions
        {
            AutoScheduleApiKey = "wsw_autoapi_test",
            MaxRetries = maxRetries,
        });
}
