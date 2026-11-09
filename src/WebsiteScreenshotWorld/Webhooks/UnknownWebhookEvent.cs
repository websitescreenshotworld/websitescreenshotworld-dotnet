using System.Text.Json;

namespace WebsiteScreenshotWorld.Webhooks;

/// <summary>
/// An event type this version of the library does not know yet. The signature was verified; the data is left as
/// JSON.
/// </summary>
/// <param name="Id">The delivery id.</param>
/// <param name="Type">The event type.</param>
/// <param name="CreatedAt">When the event happened, in UTC.</param>
/// <param name="Product">The product that sent it.</param>
/// <param name="Data">The event's <c>data</c> as sent.</param>
public sealed record UnknownWebhookEvent(string Id, string Type, DateTimeOffset CreatedAt, WebhookProduct Product, JsonElement Data)
    : WebhookEvent(Id, Type, CreatedAt, Product);
