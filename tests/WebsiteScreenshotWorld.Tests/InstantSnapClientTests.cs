using System.Net;
using System.Text.Json;
using WebsiteScreenshotWorld.InstantSnap;
using WebsiteScreenshotWorld.Tests.Support;

namespace WebsiteScreenshotWorld.Tests;

public sealed class InstantSnapClientTests
{
    private const string CaptureJson = """
        {
          "success": true,
          "id": "3f6c2a1e-9b8d-4c7e-a5f4-1d2e3c4b5a69",
          "status": "succeeded",
          "response_type": "json",
          "media_category": "image",
          "format": "avif",
          "captured_at": "2026-09-26T10:30:04Z",
          "expires_at": "2026-09-27T16:00:04.5123456789+05:30",
          "download_url": "https://files.example.com/0.png?s=abc",
          "a_field_added_later": { "nested": true }
        }
        """;

    private const string QueuedJson = """
        {
          "success": true,
          "id": "3f6c2a1e-9b8d-4c7e-a5f4-1d2e3c4b5a69",
          "status": "queued",
          "response_type": "json",
          "media_category": null,
          "format": null
        }
        """;

    [Fact]
    public async Task UserAgentNamesLibraryVersionAndRuntime()
    {
        var handler = StubHandler.Json(CaptureJson);
        using var client = Client(handler);

        await client.InstantSnap.GetAsync(Guid.NewGuid());

        var userAgent = handler.Requests.Single().Headers["User-Agent"];
        Assert.Matches(@"^websitescreenshotworld-dotnet/0\.1\.0 \(\.NET [^ ]+.*\)$", userAgent);
    }

    [Fact]
    public async Task DatesParseLenientlyAsUtcAndUnknownFieldsAreIgnored()
    {
        using var client = Client(StubHandler.Json(CaptureJson));

        var capture = await client.InstantSnap.GetAsync(Guid.NewGuid());

        Assert.Equal(new DateTimeOffset(2026, 9, 26, 10, 30, 4, TimeSpan.Zero), capture.CapturedAt);
        Assert.Equal(new DateTimeOffset(2026, 9, 27, 10, 30, 4, TimeSpan.Zero).AddTicks(5123456), capture.ExpiresAt);
        Assert.Equal(TimeSpan.Zero, capture.ExpiresAt!.Value.Offset);
    }

    [Fact]
    public async Task UnknownTokenValuesComeThrough()
    {
        using var client = Client(StubHandler.Json(CaptureJson));

        var capture = await client.InstantSnap.GetAsync(Guid.NewGuid());

        Assert.Equal(CaptureStatus.Succeeded, capture.Status);
        Assert.Equal("avif", capture.Format.ToString());
        Assert.NotEqual(CaptureFormat.Png, capture.Format);
    }

    [Fact]
    public async Task DownloadWritesFileWithoutSendingApiKey()
    {
        var bytes = new byte[] { 1, 2, 3, 4, 5 };
        var handler = new StubHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(bytes) }));
        using var client = Client(handler);
        var capture = new Capture { DownloadUrl = new Uri("https://files.example.com/0.png?s=abc") };
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        try
        {
            await client.InstantSnap.DownloadAsync(capture, path);

            Assert.Equal(bytes, await File.ReadAllBytesAsync(path));
            var request = handler.Requests.Single();
            Assert.Equal(capture.DownloadUrl, request.Uri);
            Assert.False(request.Headers.ContainsKey("X-Api-Key"));
            Assert.False(request.Headers.ContainsKey("Accept"));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task DownloadCopiesIntoStream()
    {
        var handler = new StubHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent([9, 8, 7]) }));
        using var client = Client(handler);
        using var destination = new MemoryStream();

        await client.InstantSnap.DownloadAsync(new Capture { DownloadUrl = new Uri("https://files.example.com/1.png") }, destination);

        Assert.Equal(new byte[] { 9, 8, 7 }, destination.ToArray());
    }

    [Fact]
    public async Task DownloadWithoutLinkFailsBeforeSending()
    {
        var handler = StubHandler.Json("{}");
        using var client = Client(handler);

        await Assert.ThrowsAsync<InvalidOperationException>(() => client.InstantSnap.DownloadAsync(new Capture(), Stream.Null));
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task FailedDownloadKeepsExistingFileAndLeavesNothingBehind()
    {
        var handler = new StubHandler((_, _) => Task.FromResult(StubHandler.JsonResponse("""{"success":false,"error_code":"not_found","error_message":"Gone.","retry_after_seconds":null}""", HttpStatusCode.NotFound)));
        using var client = Client(handler);
        var directory = Directory.CreateTempSubdirectory();
        var path = Path.Combine(directory.FullName, "capture.png");
        await File.WriteAllTextAsync(path, "earlier");

        try
        {
            await Assert.ThrowsAsync<NotFoundException>(() => client.InstantSnap.DownloadAsync(new Capture { DownloadUrl = new Uri("https://files.example.com/2.png") }, path));

            Assert.Equal("earlier", await File.ReadAllTextAsync(path));
            Assert.Single(directory.GetFiles());
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task CaptureFileDoesNotAskForJson()
    {
        var handler = new StubHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent([1]) }));
        using var client = Client(handler);

        await using var file = await client.InstantSnap.CaptureFileAsync(new CaptureOptions { Url = "https://example.com" });

        Assert.False(handler.Requests.Single().Headers.ContainsKey("Accept"));
    }

    [Fact]
    public async Task CaptureToBucketRequiresDestination()
    {
        var handler = StubHandler.Json("{}");
        using var client = Client(handler);

        await Assert.ThrowsAsync<ArgumentException>(() => client.InstantSnap.CaptureToBucketAsync(new CaptureOptions { Url = "https://example.com" }));
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task CaptureRequiresUrl()
    {
        using var client = Client(StubHandler.Json("{}"));

        await Assert.ThrowsAsync<ArgumentException>(() => client.InstantSnap.CaptureAsync(new CaptureOptions()));
    }

    [Fact]
    public async Task EachCaptureCallGetsItsOwnIdempotencyKey()
    {
        var handler = StubHandler.Json(CaptureJson);
        using var client = Client(handler);

        await client.InstantSnap.CaptureAsync(new CaptureOptions { Url = "https://example.com" });
        await client.InstantSnap.CaptureAsync(new CaptureOptions { Url = "https://example.com" });

        var keys = handler.Requests.Select(r => r.Headers["Idempotency-Key"]).ToArray();
        Assert.All(keys, key => Assert.True(Guid.TryParse(key, out _)));
        Assert.NotEqual(keys[0], keys[1]);
    }

    [Fact]
    public async Task SubmitSendsAsyncAndAcceptsQueuedAnswer()
    {
        var handler = StubHandler.Json(QueuedJson, HttpStatusCode.Accepted);
        using var client = Client(handler);

        var capture = await client.InstantSnap.SubmitAsync(new CaptureOptions { Url = "https://example.com", TimeoutSeconds = 240 });

        Assert.Equal(CaptureStatus.Queued, capture.Status);
        Assert.Null(capture.MediaCategory);
        var request = handler.Requests.Single();
        Assert.True(Guid.TryParse(request.Headers["Idempotency-Key"], out _));
        using var expected = JsonDocument.Parse("""{"url":"https://example.com","timeout":240,"response_type":"json","async":true}""");
        using var sent = JsonDocument.Parse(request.Body!);
        JsonAssert.Equal(expected.RootElement, sent.RootElement);
    }

    [Fact]
    public async Task SubmitToBucketRequiresDestination()
    {
        var handler = StubHandler.Json("{}");
        using var client = Client(handler);

        await Assert.ThrowsAsync<ArgumentException>(() => client.InstantSnap.SubmitToBucketAsync(new CaptureOptions { Url = "https://example.com" }));
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task QueueFullIsRaisedAsRateLimitAndNotRetried()
    {
        var handler = new StubHandler((_, _) => Task.FromResult(StubHandler.JsonResponse(
            """{"success":false,"error_code":"queue_full","error_message":"Too many waiting.","retry_after_seconds":30}""",
            HttpStatusCode.TooManyRequests)));
        using var client = Client(handler);

        var exception = await Assert.ThrowsAsync<RateLimitException>(() => client.InstantSnap.SubmitAsync(new CaptureOptions { Url = "https://example.com" }));

        Assert.Single(handler.Requests);
        Assert.Equal("queue_full", exception.ErrorCode);
        Assert.Equal(TimeSpan.FromSeconds(30), exception.RetryAfter);
    }

    [Fact]
    public async Task WaitReturnsLastCaptureWhenTimeRunsOut()
    {
        var handler = StubHandler.Json(QueuedJson);
        using var client = Client(handler);

        var capture = await client.InstantSnap.WaitAsync(Guid.NewGuid(), TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(20));

        Assert.Equal(CaptureStatus.Queued, capture.Status);
        Assert.True(handler.Requests.Count >= 2, $"Read {handler.Requests.Count} times.");
    }

    [Fact]
    public async Task WaitWithNoTimeLeftReadsOnce()
    {
        var handler = StubHandler.Json(QueuedJson);
        using var client = Client(handler);

        var capture = await client.InstantSnap.WaitAsync(Guid.NewGuid(), TimeSpan.Zero, TimeSpan.Zero);

        Assert.Equal(CaptureStatus.Queued, capture.Status);
        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task WaitStopsAtOnceWhenCancelled()
    {
        var handler = StubHandler.Json(QueuedJson);
        using var client = Client(handler);
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => client.InstantSnap.WaitAsync(Guid.NewGuid(), pollInterval: TimeSpan.FromSeconds(30), cancellationToken: cancellation.Token));

        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task WaitPassesErrorsFromGetThrough()
    {
        var handler = StubHandler.Json(
            """{"success":false,"error_code":"capture_not_found","error_message":"No such capture.","retry_after_seconds":null}""",
            HttpStatusCode.NotFound);
        using var client = Client(handler);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() => client.InstantSnap.WaitAsync(Guid.NewGuid(), pollInterval: TimeSpan.Zero));

        Assert.Equal("capture_not_found", exception.ErrorCode);
    }

    private static WebsiteScreenshotWorldClient Client(StubHandler handler) =>
        new(new HttpClient(handler), new WebsiteScreenshotWorldClientOptions { InstantSnapApiKey = "wsw_api_test" });
}
