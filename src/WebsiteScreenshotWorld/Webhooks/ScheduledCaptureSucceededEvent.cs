namespace WebsiteScreenshotWorld.Webhooks;

/// <summary>An Auto Schedule capture succeeded (<c>capture.succeeded</c>).</summary>
/// <param name="Id">The delivery id.</param>
/// <param name="CreatedAt">When the event happened, in UTC.</param>
/// <param name="Data">The capture.</param>
public sealed record ScheduledCaptureSucceededEvent(string Id, DateTimeOffset CreatedAt, ScheduledCapture Data)
    : WebhookEvent(Id, "capture.succeeded", CreatedAt, WebhookProduct.AutoSchedule);
