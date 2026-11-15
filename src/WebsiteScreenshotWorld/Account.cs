using System.Text.Json.Serialization;
using WebsiteScreenshotWorld.Webhooks;

namespace WebsiteScreenshotWorld;

/// <summary>
/// Whose API key this is: the answer to <see cref="InstantSnap.InstantSnapClient.GetAccountAsync"/> and
/// <see cref="AutoSchedule.AutoScheduleClient.GetAccountAsync"/>. It never includes the key itself.
/// </summary>
public sealed class Account
{
    /// <summary>The name given to the key in the dashboard (Developer Tools, API Keys), if it has one.</summary>
    [JsonPropertyName("key_name")]
    public string? KeyName { get; init; }

    /// <summary>The key's public prefix, as the dashboard shows it. Not enough to use the key.</summary>
    [JsonPropertyName("key_prefix")]
    public string KeyPrefix { get; init; } = string.Empty;

    /// <summary>The product the key belongs to.</summary>
    [JsonPropertyName("product")]
    public WebhookProduct Product { get; init; }

    /// <summary>The email of the member who created the key, or <see langword="null"/> when they are no longer a member.</summary>
    [JsonPropertyName("email")]
    public string? Email { get; init; }
}
