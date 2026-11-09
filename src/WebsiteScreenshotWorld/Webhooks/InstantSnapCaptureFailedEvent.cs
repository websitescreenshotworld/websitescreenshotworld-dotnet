using WebsiteScreenshotWorld.InstantSnap;

namespace WebsiteScreenshotWorld.Webhooks;

/// <summary>An Instant Snap capture failed (<c>capture.failed</c>).</summary>
/// <param name="Id">The delivery id.</param>
/// <param name="CreatedAt">When the event happened, in UTC.</param>
/// <param name="Data">The capture as <see cref="InstantSnapClient.GetAsync"/> returns it, with the reason in <see cref="Capture.ErrorCode"/>.</param>
public sealed record InstantSnapCaptureFailedEvent(string Id, DateTimeOffset CreatedAt, Capture Data)
    : WebhookEvent(Id, "capture.failed", CreatedAt, WebhookProduct.InstantSnap);
