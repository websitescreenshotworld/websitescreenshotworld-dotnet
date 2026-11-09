namespace WebsiteScreenshotWorld.Webhooks;

/// <summary>A sitemap schedule finished a run (<c>sitemap_run.completed</c>).</summary>
/// <param name="Id">The delivery id.</param>
/// <param name="CreatedAt">When the event happened, in UTC.</param>
/// <param name="Data">The run and its page counts.</param>
public sealed record SitemapRunCompletedEvent(string Id, DateTimeOffset CreatedAt, SitemapRun Data)
    : WebhookEvent(Id, "sitemap_run.completed", CreatedAt, WebhookProduct.AutoSchedule);
