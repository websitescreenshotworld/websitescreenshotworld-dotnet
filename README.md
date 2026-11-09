# Website Screenshot World for .NET

The official .NET client for [Website Screenshot World](https://websitescreenshotworld.com). Capture web pages as
images, videos or PDFs with **Instant Snap**, start scheduled captures with **Auto Schedule**, and verify the
webhooks both products send.

```sh
dotnet add package WebsiteScreenshotWorld
```

Using `Microsoft.Extensions.DependencyInjection` (ASP.NET Core, worker services)? Add the integration package as
well; see [Dependency injection](#dependency-injection).

```sh
dotnet add package WebsiteScreenshotWorld.Extensions.DependencyInjection
```

## Quick start

Create an API key for each product you use in the dashboard. The client reads them from the
`WSW_INSTANT_SNAP_API_KEY` and `WSW_AUTO_SCHEDULE_API_KEY` environment variables, or you can pass them in.

```csharp
using WebsiteScreenshotWorld;
using WebsiteScreenshotWorld.InstantSnap;

using var client = new WebsiteScreenshotWorldClient();

var capture = await client.InstantSnap.CaptureAsync(new CaptureOptions
{
    Url = "https://example.com",
    Format = CaptureFormat.WebP,
    Viewport = new Viewport { Width = 1440, Height = 900 },
    ScrollMode = ScrollMode.Scroll,
});

Console.WriteLine($"{capture.Id}: {capture.Width}x{capture.Height}, {capture.SizeBytes} bytes");
Console.WriteLine($"Download within the hour: {capture.DownloadUrl}");
```

Create one client and reuse it: it is safe to share between threads, and it keeps connections open between calls.

Options you leave unset are not sent, so the API applies its defaults
(`await client.InstantSnap.GetDefaultsAsync()` lists them). `false`, `0` and empty strings are sent as values.

### Auto Schedule

Web pages are scheduled in the dashboard. `TriggerNowAsync` captures one now, and the result arrives by
[webhook](#webhooks). Calling it again while a capture is pending starts nothing new.

```csharp
using WebsiteScreenshotWorld.AutoSchedule;

var result = await client.AutoSchedule.TriggerNowAsync(Guid.Parse("7d4e2b1a-6c5f-4a3e-9d8c-2b1a0f9e8d7c"));

if (result.Status == TriggerStatus.AlreadyPending)
{
    Console.WriteLine($"Already queued at {result.TriggeredAt:u}.");
}
```

## Captures

### Save the file straight away

`CaptureFileAsync` returns the file in the response. It is streamed, not held in memory, and the API does not keep
a copy, so save or read it and then dispose it.

```csharp
await using var file = await client.InstantSnap.CaptureFileAsync(new CaptureOptions
{
    Url = "https://example.com",
    MediaCategory = MediaCategory.Pdf,
    Format = CaptureFormat.DocumentPdf,
});

await file.SaveToAsync(file.FileName ?? "capture.pdf");
Console.WriteLine($"{file.ContentType}, {file.SizeBytes} bytes, SHA-256 {file.Sha256}");
```

`file.Content` is the raw `Stream` if you would rather copy it somewhere yourself, for example into a blob upload.

### Upload to your own bucket

`CaptureToBucketAsync` uploads the file to any S3-compatible bucket. Set `Region` for AWS or `EndpointUrl` for
other providers.

```csharp
var capture = await client.InstantSnap.CaptureToBucketAsync(new CaptureOptions
{
    Url = "https://example.com",
    S3Delivery = new S3DeliveryOptions
    {
        AccessKeyId = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID"),
        SecretAccessKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY"),
        Bucket = "my-captures",
        Region = "eu-west-2",
        KeyPrefix = "homepage/",
        WaitForSync = true,
    },
});

Console.WriteLine($"Upload {capture.S3Delivery?.Status}: {capture.S3Delivery?.Key}");
```

### Read a capture back

Files are kept for 24 hours and a download link lasts 60 minutes. `GetAsync` returns the capture with a fresh link,
and `DownloadAsync` saves the file to a path or a stream.

```csharp
var latest = await client.InstantSnap.GetAsync(capture.Id);

if (latest.Status == CaptureStatus.Succeeded)
{
    await client.InstantSnap.DownloadAsync(latest, "homepage.png");
}
```

### Async captures

`SubmitAsync` and `SubmitToBucketAsync` queue a capture and return at once (HTTP 202) with the capture in
`CaptureStatus.Queued`; it moves to `Running` when a browser is free, then succeeds or fails. `WaitAsync` reads it
every 3 seconds until it has finished and returns it. If it is still unfinished when the wait runs out (360 seconds
by default) you get the capture as last read rather than an exception, so check `Status` and wait again if you like.

```csharp
var queued = await client.InstantSnap.SubmitAsync(new CaptureOptions
{
    Url = "https://example.com",
    TimeoutSeconds = 240,
});

var finished = await client.InstantSnap.WaitAsync(
    queued.Id,
    timeout: TimeSpan.FromMinutes(10),
    pollInterval: TimeSpan.FromSeconds(5),
    cancellationToken: cancellationToken);

if (finished.Status == CaptureStatus.Succeeded)
{
    await client.InstantSnap.DownloadAsync(finished, "homepage.png");
}

var upload = await client.InstantSnap.SubmitToBucketAsync(new CaptureOptions
{
    Url = "https://example.com",
    S3Delivery = new S3DeliveryOptions { AccessKeyId = "...", SecretAccessKey = "...", Bucket = "my-captures", Region = "eu-west-2" },
});
```

- `TimeoutSeconds` (sent as `timeout`) is the browser time the capture may use, 30 to 300 seconds; the API uses 120
  when it is not set, and refuses it on the other capture methods. It is not `RequestOptions.Timeout`, which is how
  long the client waits for an answer: submitting takes the ordinary 30-second timeout, since nothing runs while
  you wait.
- A capture still queued 15 minutes after it was accepted ends `failed` with `capture_expired` and is not charged.
- When your organization already has as many async captures waiting or running as it may, the submit is refused
  with `queue_full`, raised as a `RateLimitException` whose `RetryAfter` says when to try again. It is not retried.
- Instead of waiting you can call `GetAsync` yourself, or pass a `CallbackUrl` and take the webhook.

### Everything else a capture can do

```csharp
var detailed = await client.InstantSnap.CaptureAsync(new CaptureOptions
{
    Url = "https://example.com/pricing",
    DeviceCategory = DeviceCategory.Mobile,
    EmulatedDevice = "Pixel 7",
    BlockAds = false,
    Locale = "en-GB",
    TimeZone = "Europe/London",
    Headers = new Dictionary<string, string> { ["X-Preview"] = "1" },
    Cookies = new Dictionary<string, string> { ["theme"] = "dark" },
    Geolocation = new Geolocation { Latitude = 51.5072, Longitude = -0.1276 },
    Selectors = new Selectors { Hide = ".cookie-banner" },
    Proxy = new ProxyOptions { Country = "gb" },
    TimestampWatermark = new TimestampWatermark { Location = WatermarkLocation.Bottom },
    PageSource = new PageSourceOptions { Format = PageSourceFormat.Html },
    CallbackUrl = "https://example.com/webhooks/website-screenshot-world",
});

var catalog = await client.InstantSnap.GetCatalogAsync();
Console.WriteLine(string.Join(", ", catalog.EmulatedDevices));
```

Header and cookie names are sent exactly as you write them. Values such as `CaptureFormat` and `CaptureStatus` are
small structs rather than enums, so a value the API adds later still comes through; compare them with the static
properties (`capture.Status == CaptureStatus.Failed`).

## Errors and retries

Every error derives from `WebsiteScreenshotWorldException`. A refusal from the API carries its `ErrorCode`,
`StatusCode`, `RetryAfter` and the raw `ResponseBody`, and is raised as the subclass for its kind:

| Exception | Error codes |
|---|---|
| `AuthenticationException` | `api_key_invalid` |
| `InvalidRequestException` | `validation_failed`, `s3_destination_invalid`, `web_page_id_invalid`, `idempotency_key_reused`, `sitemap_schedule_not_triggerable` |
| `PlanLimitException` | `quota_exhausted`, `feature_not_available`, `no_trigger_allowance`, `forbidden` |
| `NotFoundException` | `capture_not_found`, `web_page_not_found`, `not_found` |
| `RateLimitException` | `rate_limited`, `queue_full` |
| `CaptureException` | `capture_failed`, `capture_timeout`, `payload_too_large`, `capture_expired` |
| `ServiceException` | `capture_busy`, `service_unavailable`, `internal_error`, `request_cancelled`, `idempotency_key_in_use` |
| `WebsiteScreenshotWorldConnectionException` | none: no answer came back (see `InnerException`) |
| `WebsiteScreenshotWorldException` | a code this version does not know yet |

Decide what to do from `ErrorCode`; the status and the message can change.

```csharp
try
{
    await client.InstantSnap.CaptureAsync(new CaptureOptions { Url = "https://example.com" });
}
catch (CaptureException e) when (e.ErrorCode == "capture_timeout")
{
    Console.WriteLine("The page took too long; try a longer PageTimeoutMs.");
}
catch (PlanLimitException e)
{
    Console.WriteLine($"Plan limit: {e.ErrorCode}");
}
catch (WebsiteScreenshotWorldConnectionException e)
{
    Console.WriteLine($"No answer: {e.InnerException?.Message}");
}
catch (WebsiteScreenshotWorldException e)
{
    Console.WriteLine($"{e.ErrorCode} (HTTP {(int?)e.StatusCode}): {e.Message}");
}
```

A call with no API key for its product throws `InvalidOperationException` before anything is sent.

The client retries `rate_limited`, `capture_busy`, `service_unavailable` and `idempotency_key_in_use` answers, and
connection failures, up to twice (`MaxRetries`). It waits as long as the API asks, up to 60 seconds, or otherwise
0.5 seconds and then 1 second with a little jitter. Nothing else is retried: a `capture_failed` page will fail the
same way again, and a `queue_full` queue will still be full, so it is raised with `RetryAfter` for you. Cancelling the `CancellationToken` stops at once, retries included.

### Idempotency keys

Every capture sends an `Idempotency-Key`, so a retry never captures twice. The client makes a new one per call and
reuses it on that call's retries. Pass your own to make a capture safe to repeat across calls or processes: within
24 hours, the same key returns the first capture instead of taking another.

```csharp
var order = await client.InstantSnap.CaptureAsync(
    new CaptureOptions { Url = "https://example.com/order/1234" },
    new RequestOptions { IdempotencyKey = "order-1234-receipt" });
```

### Timeouts and cancellation

A capture may take 180 seconds, since the API can spend up to 120 seconds on a slow page, and any other call,
including `SubmitAsync` and `SubmitToBucketAsync`, 30 seconds. The time covers the whole call, retries and the waits
between them included, until the answer starts to arrive; reading a file from `CaptureFileAsync` afterwards is not
timed. Change them for the client, or for one call:

```csharp
var quick = await client.InstantSnap.CaptureAsync(
    new CaptureOptions { Url = "https://example.com" },
    new RequestOptions { Timeout = TimeSpan.FromSeconds(60) },
    cancellationToken);
```

A call that runs out of time is not retried: it is raised as `WebsiteScreenshotWorldConnectionException` with a
`TimeoutException` inside. A retry whose wait would run past the time is not started; the refusal is raised instead.
Cancelling your token raises `OperationCanceledException` as usual.

## Webhooks

Deliveries carry an `X-WSW-Signature` header. `Webhook.Verify` checks it against your endpoint's signing secret,
refuses a timestamp more than five minutes from now, and returns the event typed by its `type`:

| Event | Type | `Data` |
|---|---|---|
| Instant Snap `capture.succeeded` / `capture.failed` | `InstantSnapCaptureSucceededEvent` / `InstantSnapCaptureFailedEvent` | `Capture` |
| Auto Schedule `capture.succeeded` / `capture.failed` | `ScheduledCaptureSucceededEvent` / `ScheduledCaptureFailedEvent` | `ScheduledCapture` |
| `delivery.completed` | `DeliveryCompletedEvent` | `DeliveredCapture` |
| `capture.changed` | `CaptureChangedEvent` | `ChangedCapture` |
| `schedule.paused` | `SchedulePausedEvent` | `SchedulePause` |
| `sitemap_run.completed` | `SitemapRunCompletedEvent` | `SitemapRun` |
| `ping` | `PingEvent` | `WebhookPing` |
| a type added later | `UnknownWebhookEvent` | `JsonElement` |

Verify the body **exactly as received**. If your framework parses the JSON and you serialise it again, the bytes
change and the signature no longer matches, so read the raw request body as shown below. A failed check throws
`SignatureVerificationException`; answer it with 400.

### ASP.NET Core minimal APIs

```csharp
using WebsiteScreenshotWorld;
using WebsiteScreenshotWorld.Webhooks;

app.MapPost("/webhooks/website-screenshot-world", async (HttpRequest request, IConfiguration configuration) =>
{
    using var body = new MemoryStream();
    await request.Body.CopyToAsync(body);

    WebhookEvent webhookEvent;
    try
    {
        webhookEvent = Webhook.Verify(
            body.ToArray(),
            request.Headers[Webhook.SignatureHeaderName].ToString(),
            configuration["WebsiteScreenshotWorld:WebhookSecret"]!);
    }
    catch (SignatureVerificationException)
    {
        return Results.BadRequest();
    }

    switch (webhookEvent)
    {
        case InstantSnapCaptureSucceededEvent succeeded:
            Console.WriteLine($"Capture {succeeded.Data.Id} is ready.");
            break;
        case ScheduledCaptureFailedEvent failed:
            Console.WriteLine($"{failed.Data.ScheduleName} failed with HTTP {failed.Data.PageHttpStatus}.");
            break;
        case CaptureChangedEvent changed:
            Console.WriteLine($"{changed.Data.Url} changed by {changed.Data.ChangePercent}%.");
            break;
    }

    return Results.Ok();
});
```

### ASP.NET Core MVC controllers

Do not bind the body to a `[FromBody]` parameter; read `Request.Body` yourself.

```csharp
using Microsoft.AspNetCore.Mvc;
using WebsiteScreenshotWorld;
using WebsiteScreenshotWorld.Webhooks;

[ApiController]
[Route("webhooks/website-screenshot-world")]
public sealed class WebsiteScreenshotWorldWebhookController(IConfiguration configuration) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Receive()
    {
        using var body = new MemoryStream();
        await Request.Body.CopyToAsync(body, HttpContext.RequestAborted);

        try
        {
            var webhookEvent = Webhook.Verify(
                body.ToArray(),
                Request.Headers[Webhook.SignatureHeaderName].ToString(),
                configuration["WebsiteScreenshotWorld:WebhookSecret"]!);

            if (webhookEvent is DeliveryCompletedEvent delivered)
            {
                foreach (var destination in delivered.Data.Destinations)
                {
                    Console.WriteLine($"{destination.Name}: {destination.Status}");
                }
            }

            return Ok();
        }
        catch (SignatureVerificationException)
        {
            return BadRequest();
        }
    }
}
```

### Azure Functions (isolated worker)

```csharp
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using WebsiteScreenshotWorld;
using WebsiteScreenshotWorld.Webhooks;

public sealed class WebsiteScreenshotWorldWebhook
{
    [Function("WebsiteScreenshotWorldWebhook")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData request)
    {
        using var body = new MemoryStream();
        await request.Body.CopyToAsync(body);
        var signature = request.Headers.TryGetValues(Webhook.SignatureHeaderName, out var values) ? values.First() : "";

        try
        {
            var webhookEvent = Webhook.Verify(body.ToArray(), signature, Environment.GetEnvironmentVariable("WSW_WEBHOOK_SECRET")!);
            if (webhookEvent is SchedulePausedEvent paused)
            {
                Console.WriteLine($"{paused.Data.ScheduleName} paused: {paused.Data.Reason}");
            }

            return request.CreateResponse(HttpStatusCode.OK);
        }
        catch (SignatureVerificationException)
        {
            return request.CreateResponse(HttpStatusCode.BadRequest);
        }
    }
}
```

`Webhook.Verify` also accepts the body as a `string`, a custom `tolerance`, and a `now` for tests.

## Dependency injection

`AddWebsiteScreenshotWorld` registers `WebsiteScreenshotWorldClient` as a typed client, so its `HttpClient` comes
from `IHttpClientFactory`. It returns the `IHttpClientBuilder`, so you can add handlers as with any typed client.

```csharp
builder.Services.AddWebsiteScreenshotWorld(options =>
{
    options.InstantSnapApiKey = builder.Configuration["WebsiteScreenshotWorld:InstantSnapApiKey"];
    options.AutoScheduleApiKey = builder.Configuration["WebsiteScreenshotWorld:AutoScheduleApiKey"];
});
```

Or bind every setting from configuration:

```csharp
builder.Services.AddWebsiteScreenshotWorld();
builder.Services.Configure<WebsiteScreenshotWorldClientOptions>(builder.Configuration.GetSection("WebsiteScreenshotWorld"));
```

Then take `WebsiteScreenshotWorldClient` in a constructor. The client already retries, so do not add a retry
handler on top. Its handler does not follow redirects; if you replace the primary handler, keep
`AllowAutoRedirect = false` on yours.

If you pass your own `HttpClient` to the constructor instead, set its `Timeout` to `Timeout.InfiniteTimeSpan`: the
client applies its own, longer timeouts, and the default of 100 seconds would cut long captures short. Turn
redirects off in its handler too:

```csharp
var http = new HttpClient(new SocketsHttpHandler { AllowAutoRedirect = false }) { Timeout = Timeout.InfiniteTimeSpan };
using var client = new WebsiteScreenshotWorldClient(http);
```

A handler that follows a redirect sends the API key on to wherever it points. Neither the API nor a download link
redirects, so the client refuses any answer that came through a redirect, but by then the key has been sent.

## Configuration

| `WebsiteScreenshotWorldClientOptions` | Default |
|---|---|
| `InstantSnapApiKey` | the `WSW_INSTANT_SNAP_API_KEY` environment variable |
| `AutoScheduleApiKey` | the `WSW_AUTO_SCHEDULE_API_KEY` environment variable |
| `InstantSnapBaseUrl` | `https://api.websitescreenshotworld.com` |
| `AutoScheduleBaseUrl` | `https://autoapi.websitescreenshotworld.com` |
| `CaptureTimeout` | 180 seconds per call, retries included |
| `Timeout` | 30 seconds per call, retries included, for everything but synchronous captures (async submits use it) |
| `MaxRetries` | 2, so at most 3 attempts |

Either key may be left out if you only use the other product. `GetDefaultsAsync` needs no key.

## Supported runtimes

.NET 8 and later, tested on .NET 8 and .NET 10. The package also carries a .NET Standard 2.0 build for
.NET Framework 4.6.2 and later and other runtimes that implement it.

## Development

The tests play the shared cases in [`spec/`](https://github.com/websitescreenshotworld/websitescreenshotworld-spec) through [`spec/conformance/server.js`](https://github.com/websitescreenshotworld/websitescreenshotworld-spec/blob/main/conformance/server.js),
so they need Node.js 22 or later on the `PATH`. `spec/` is the [websitescreenshotworld-spec](https://github.com/websitescreenshotworld/websitescreenshotworld-spec) submodule: clone with `git clone --recurse-submodules`, or run `git submodule update --init` in an existing clone.

```sh
dotnet test
dotnet format --verify-no-changes
```

## License

MIT. See [LICENSE](https://github.com/websitescreenshotworld/websitescreenshotworld-dotnet/blob/main/LICENSE).
