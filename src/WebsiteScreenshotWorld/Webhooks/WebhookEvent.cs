namespace WebsiteScreenshotWorld.Webhooks;

/// <summary>
/// A verified webhook delivery. Match on the derived type to read its data; a type this version of the library
/// does not know arrives as <see cref="UnknownWebhookEvent"/>.
/// </summary>
/// <param name="Id">The delivery id.</param>
/// <param name="Type">The event type, such as <c>capture.succeeded</c>.</param>
/// <param name="CreatedAt">When the event happened, in UTC.</param>
/// <param name="Product">The product that sent it.</param>
public abstract record WebhookEvent(string Id, string Type, DateTimeOffset CreatedAt, WebhookProduct Product);
