# Changelog

All notable changes to the .NET library are recorded here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and versions follow
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## 0.3.0 - 2026-11-15

Follows the API spec at Instant Snap API 1.4.0 and Auto Schedule API 1.3.0.

### Added

- `InstantSnap.GetAccountAsync` and `AutoSchedule.GetAccountAsync`: whose key it is, as an `Account` with the key's
  name and prefix, its product and the email of the member who created it. It never includes the key and costs no
  capture, so it is also a cheap way to check a key works.

## 0.2.0 - 2026-11-15

Follows the API spec at Instant Snap API 1.3.0.

### Added

- `Capture.Url`: the page the capture was asked for, on every capture and on Instant Snap webhook events, as
  Auto Schedule events already had it.
- The error codes of the webhook endpoints API: `webhook_endpoint_not_found` throws `NotFoundException` and
  `webhook_endpoint_limit_reached` throws `PlanLimitException`.

## 0.1.0 - 2026-11-09

The first release.

### Added

- `WebsiteScreenshotWorldClient` covering both products, with keys read from `WSW_INSTANT_SNAP_API_KEY` and
  `WSW_AUTO_SCHEDULE_API_KEY` when not set, and an overload that takes your own `HttpClient`.
- Instant Snap: `CaptureAsync`, `CaptureFileAsync` (streams the file with its metadata), `CaptureToBucketAsync`,
  `GetAsync`, `DownloadAsync` (to a path or a stream), `GetDefaultsAsync` and `GetCatalogAsync`, with every
  capture option in the API.
- Instant Snap async captures: `SubmitAsync` and `SubmitToBucketAsync` (answered at once, `202`, status
  `queued`) and `WaitAsync` (polls `GetAsync` until the capture is finished, returning it as last read when the
  wait runs out); the `CaptureOptions.TimeoutSeconds` option (sent as `timeout`, 30-300 seconds, async only); the
  `CaptureStatus.Queued` and `CaptureStatus.Running` statuses; the `queue_full` (`RateLimitException`, not
  retried) and `capture_expired` (`CaptureException`) codes. `Capture.MediaCategory` is now nullable, as a
  queued or running capture has none yet.
- Auto Schedule: `TriggerNowAsync`.
- The `PressEscape` capture option, and the `sitemap_schedule_not_triggerable` error code (an
  `InvalidRequestException`: a sitemap schedule runs only on its schedule), from API version 1.1.0.
- `Webhook.Verify` for signed deliveries, returning typed events for every event type and
  `UnknownWebhookEvent` for types added later.
- An exception per error kind under `WebsiteScreenshotWorldException`, and
  `WebsiteScreenshotWorldConnectionException` when no answer came back.
- Timeouts that bound the whole call, retries and the waits between them included (180 seconds for captures,
  30 seconds for everything else); a call that runs out of time is not retried.
- Redirects are never followed by the client's own `HttpClient` or the one `AddWebsiteScreenshotWorld` registers, so
  the API key is never sent to another host; a 3xx answer is raised as an error, and an answer that came through a
  redirect followed by an `HttpClient` you passed in is refused.
- Automatic retries of rate-limited, busy and unavailable answers and of connection failures, honouring
  `Retry-After`, with an `Idempotency-Key` on every capture so a retry never captures twice.
- Per-call idempotency keys and timeouts through `RequestOptions`.
- Targets .NET 8 and .NET Standard 2.0.
- `WebsiteScreenshotWorld.Extensions.DependencyInjection` package: `services.AddWebsiteScreenshotWorld(...)`
  registers the client as a typed `HttpClient`.
