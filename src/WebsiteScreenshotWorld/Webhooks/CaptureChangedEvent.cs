namespace WebsiteScreenshotWorld.Webhooks;

/// <summary>An Auto Schedule capture differs from the one before it (<c>capture.changed</c>).</summary>
/// <param name="Id">The delivery id.</param>
/// <param name="CreatedAt">When the event happened, in UTC.</param>
/// <param name="Data">The capture and how much it changed.</param>
public sealed record CaptureChangedEvent(string Id, DateTimeOffset CreatedAt, ChangedCapture Data)
    : WebhookEvent(Id, "capture.changed", CreatedAt, WebhookProduct.AutoSchedule);
