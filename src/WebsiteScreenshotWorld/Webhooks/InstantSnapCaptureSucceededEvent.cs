using WebsiteScreenshotWorld.InstantSnap;

namespace WebsiteScreenshotWorld.Webhooks;

/// <summary>An Instant Snap capture succeeded (<c>capture.succeeded</c>).</summary>
/// <param name="Id">The delivery id.</param>
/// <param name="CreatedAt">When the event happened, in UTC.</param>
/// <param name="Data">The capture as <see cref="InstantSnapClient.GetAsync"/> returns it, without download links or the bucket access key id.</param>
public sealed record InstantSnapCaptureSucceededEvent(string Id, DateTimeOffset CreatedAt, Capture Data)
    : WebhookEvent(Id, "capture.succeeded", CreatedAt, WebhookProduct.InstantSnap);
