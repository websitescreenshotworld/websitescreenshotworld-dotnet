namespace WebsiteScreenshotWorld.Webhooks;

/// <summary>An Auto Schedule capture was sent to its destinations (<c>delivery.completed</c>).</summary>
/// <param name="Id">The delivery id.</param>
/// <param name="CreatedAt">When the event happened, in UTC.</param>
/// <param name="Data">The capture and the outcome of each delivery.</param>
public sealed record DeliveryCompletedEvent(string Id, DateTimeOffset CreatedAt, DeliveredCapture Data)
    : WebhookEvent(Id, "delivery.completed", CreatedAt, WebhookProduct.AutoSchedule);
