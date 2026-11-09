using System.Security.Cryptography;
using System.Text.Json;
using WebsiteScreenshotWorld.InstantSnap;
using WebsiteScreenshotWorld.Tests.Support;

namespace WebsiteScreenshotWorld.Tests;

/// <summary>Plays every fixture in <c>spec/fixtures</c> against the client through the conformance server.</summary>
public sealed class ConformanceTests(ConformanceServer server) : IClassFixture<ConformanceServer>
{
    private static readonly Dictionary<string, Type> ErrorKinds = new()
    {
        ["base"] = typeof(WebsiteScreenshotWorldException),
        ["authentication"] = typeof(AuthenticationException),
        ["invalid_request"] = typeof(InvalidRequestException),
        ["plan_limit"] = typeof(PlanLimitException),
        ["not_found"] = typeof(NotFoundException),
        ["rate_limit"] = typeof(RateLimitException),
        ["capture"] = typeof(CaptureException),
        ["service"] = typeof(ServiceException),
    };

    public static TheoryData<string> Fixtures()
    {
        var names = Directory.GetFiles(RepoPaths.Spec("fixtures"), "*.json")
            .Select(Path.GetFileName)
            .Where(name => name != "keys.json")
            .Order(StringComparer.Ordinal);
        return new TheoryData<string>(names!);
    }

    [Theory]
    [MemberData(nameof(Fixtures))]
    public async Task FixtureBehavesAsSpecified(string fileName)
    {
        using var document = JsonDocument.Parse(await File.ReadAllTextAsync(RepoPaths.Spec("fixtures", fileName)));
        var fixture = document.RootElement;
        await server.LoadAsync(fixture);

        using var client = CreateClient();
        var (result, error) = await CallAsync(client, fixture.GetProperty("call"));

        AssertRequests(fixture.GetProperty("expect_requests"), await server.RequestsAsync());

        var expect = fixture.GetProperty("expect");
        if (expect.TryGetProperty("error", out var expectedError))
        {
            AssertError(expectedError, error);
        }
        else
        {
            Assert.Null(error);
            JsonAssert.Subset(expect.GetProperty("result"), result);
        }
    }

    private static async Task<(JsonElement Result, Exception? Error)> CallAsync(WebsiteScreenshotWorldClient client, JsonElement call)
    {
        var requestOptions = call.TryGetProperty("idempotency_key", out var key)
            ? new RequestOptions { IdempotencyKey = key.GetString() }
            : null;
        var method = call.GetProperty("method").GetString();

        try
        {
            return method switch
            {
                "capture" => (JsonAssert.ToWire(await client.InstantSnap.CaptureAsync(Options(call), requestOptions)), null),
                "capture_file" => (await CaptureFileAsync(client, Options(call), requestOptions), null),
                "capture_to_bucket" => (JsonAssert.ToWire(await client.InstantSnap.CaptureToBucketAsync(Options(call), requestOptions)), null),
                "submit" => (JsonAssert.ToWire(await client.InstantSnap.SubmitAsync(Options(call), requestOptions)), null),
                "submit_to_bucket" => (JsonAssert.ToWire(await client.InstantSnap.SubmitToBucketAsync(Options(call), requestOptions)), null),
                "wait" => (JsonAssert.ToWire(await client.InstantSnap.WaitAsync(
                    Guid.Parse(call.GetProperty("id").GetString()!),
                    Seconds(call, "timeout_seconds"),
                    Seconds(call, "poll_interval_seconds"),
                    requestOptions)), null),
                "get" => (JsonAssert.ToWire(await client.InstantSnap.GetAsync(Guid.Parse(call.GetProperty("id").GetString()!), requestOptions)), null),
                "defaults" => (JsonAssert.ToWire(await client.InstantSnap.GetDefaultsAsync(requestOptions)), null),
                "catalog" => (JsonAssert.ToWire(await client.InstantSnap.GetCatalogAsync(requestOptions)), null),
                "trigger_now" => (JsonAssert.ToWire(await client.AutoSchedule.TriggerNowAsync(Guid.Parse(call.GetProperty("web_page_id").GetString()!), requestOptions)), null),
                _ => throw new InvalidOperationException($"Unknown fixture method {method}."),
            };
        }
        catch (WebsiteScreenshotWorldException exception)
        {
            return (default, exception);
        }
    }

    private static async Task<JsonElement> CaptureFileAsync(WebsiteScreenshotWorldClient client, CaptureOptions options, RequestOptions? requestOptions)
    {
        await using var file = await client.InstantSnap.CaptureFileAsync(options, requestOptions);
        using var buffer = new MemoryStream();
        await file.SaveToAsync(buffer);

        return JsonAssert.ToWire(new Dictionary<string, object?>
        {
            ["id"] = file.Id,
            ["status"] = file.Status?.ToString(),
            ["content_type"] = file.ContentType,
            ["width"] = file.Width,
            ["height"] = file.Height,
            ["size_bytes"] = file.SizeBytes,
            ["duration_ms"] = file.DurationMs,
            ["page_http_status"] = file.PageHttpStatus,
            ["spec_version"] = file.SpecVersion,
            ["captured_at"] = file.CapturedAt,
            ["final_url"] = file.FinalUrl,
            ["sha256"] = file.Sha256,
            ["part_count"] = file.PartCount,
            ["filename"] = file.FileName,
            ["body_sha256"] = Convert.ToHexString(SHA256.HashData(buffer.ToArray())).ToLowerInvariant(),
        });
    }

    private static TimeSpan? Seconds(JsonElement call, string name) =>
        call.TryGetProperty(name, out var seconds) ? TimeSpan.FromSeconds(seconds.GetDouble()) : null;

    private static CaptureOptions Options(JsonElement call) =>
        call.GetProperty("options").Deserialize<CaptureOptions>()!;

    private static void AssertRequests(JsonElement expected, JsonElement actual)
    {
        Assert.Equal(expected.GetArrayLength(), actual.GetArrayLength());

        var idempotencyKeys = new List<string?>();
        string? idempotencyRule = null;
        foreach (var (want, got) in expected.EnumerateArray().Zip(actual.EnumerateArray()))
        {
            Assert.Equal(want.GetProperty("method").GetString(), got.GetProperty("method").GetString());
            Assert.Equal(want.GetProperty("path").GetString(), got.GetProperty("path").GetString());

            var headers = got.GetProperty("headers").EnumerateObject()
                .ToDictionary(h => h.Name, h => h.Value.GetString(), StringComparer.OrdinalIgnoreCase);
            foreach (var header in want.GetProperty("headers").EnumerateObject())
            {
                Assert.True(headers.TryGetValue(header.Name, out var value), $"Header {header.Name} was not sent.");
                Assert.Equal(header.Value.GetString(), value);
            }

            Assert.Matches(@"^websitescreenshotworld-dotnet/0\.1\.0 \(.+ .+\)$", headers["User-Agent"]);

            if (want.TryGetProperty("body", out var body))
            {
                var sent = got.GetProperty("body");
                if (body.ValueKind == JsonValueKind.Null)
                {
                    Assert.Equal(JsonValueKind.Null, sent.ValueKind);
                }
                else
                {
                    using var sentBody = JsonDocument.Parse(sent.GetString()!);
                    JsonAssert.Equal(body, sentBody.RootElement);
                }
            }

            if (want.TryGetProperty("idempotency_key", out var rule))
            {
                idempotencyRule = rule.GetString();
                headers.TryGetValue("Idempotency-Key", out var key);
                Assert.False(string.IsNullOrEmpty(key), "No Idempotency-Key was sent.");
                idempotencyKeys.Add(key);
            }
        }

        if (idempotencyRule == "same")
        {
            Assert.Single(idempotencyKeys.Distinct());
        }
    }

    private static void AssertError(JsonElement expected, Exception? error)
    {
        Assert.NotNull(error);
        var exception = Assert.IsAssignableFrom<WebsiteScreenshotWorldException>(error);
        Assert.Equal(ErrorKinds[expected.GetProperty("kind").GetString()!], exception.GetType());
        Assert.Equal(expected.GetProperty("error_code").GetString(), exception.ErrorCode);
        Assert.Equal(expected.GetProperty("http_status").GetInt32(), (int?)exception.StatusCode);

        if (expected.TryGetProperty("message", out var message))
        {
            Assert.Equal(message.GetString(), exception.Message);
        }

        if (expected.TryGetProperty("retry_after_seconds", out var retryAfter))
        {
            Assert.Equal(retryAfter.GetDouble(), exception.RetryAfter?.TotalSeconds);
        }
    }

    private WebsiteScreenshotWorldClient CreateClient()
    {
        using var keys = JsonDocument.Parse(File.ReadAllText(RepoPaths.Spec("fixtures", "keys.json")));
        return new WebsiteScreenshotWorldClient(new WebsiteScreenshotWorldClientOptions
        {
            InstantSnapApiKey = keys.RootElement.GetProperty("instant_snap").GetString(),
            AutoScheduleApiKey = keys.RootElement.GetProperty("auto_schedule").GetString(),
            InstantSnapBaseUrl = server.BaseUrl,
            AutoScheduleBaseUrl = server.BaseUrl,
        });
    }
}
