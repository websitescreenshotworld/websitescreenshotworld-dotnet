namespace WebsiteScreenshotWorld.Webhooks;

/// <summary>A scheduled web page was paused (<c>schedule.paused</c>).</summary>
/// <param name="Id">The delivery id.</param>
/// <param name="CreatedAt">When the event happened, in UTC.</param>
/// <param name="Data">The web page and why it was paused.</param>
public sealed record SchedulePausedEvent(string Id, DateTimeOffset CreatedAt, SchedulePause Data)
    : WebhookEvent(Id, "schedule.paused", CreatedAt, WebhookProduct.AutoSchedule);
