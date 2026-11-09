namespace WebsiteScreenshotWorld.Webhooks;

/// <summary>A test delivery sent from the dashboard (<c>ping</c>). Either product can send it.</summary>
/// <param name="Id">The delivery id.</param>
/// <param name="CreatedAt">When the event happened, in UTC.</param>
/// <param name="Product">The product that sent it.</param>
/// <param name="Data">The endpoint being tested.</param>
public sealed record PingEvent(string Id, DateTimeOffset CreatedAt, WebhookProduct Product, WebhookPing Data)
    : WebhookEvent(Id, "ping", CreatedAt, Product);
