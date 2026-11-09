namespace WebsiteScreenshotWorld.Webhooks;

/// <summary>An Auto Schedule capture failed (<c>capture.failed</c>).</summary>
/// <param name="Id">The delivery id.</param>
/// <param name="CreatedAt">When the event happened, in UTC.</param>
/// <param name="Data">The capture.</param>
public sealed record ScheduledCaptureFailedEvent(string Id, DateTimeOffset CreatedAt, ScheduledCapture Data)
    : WebhookEvent(Id, "capture.failed", CreatedAt, WebhookProduct.AutoSchedule);
