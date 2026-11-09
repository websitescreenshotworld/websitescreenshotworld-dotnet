using System.Text.Json.Serialization;

namespace WebsiteScreenshotWorld.InstantSnap;

/// <summary>The upload of a <c>cloud_storage</c> capture to the caller's bucket.</summary>
public sealed class CaptureS3Delivery
{
    /// <summary>The state of the upload.</summary>
    [JsonPropertyName("status")]
    public S3DeliveryStatus Status { get; init; }

    /// <summary>The bucket name.</summary>
    [JsonPropertyName("bucket")]
    public string? Bucket { get; init; }

    /// <summary>The object key, once known.</summary>
    [JsonPropertyName("key")]
    public string? Key { get; init; }

    /// <summary>The access key id, masked to its last four characters. Not included in webhooks.</summary>
    [JsonPropertyName("access_key_id")]
    public string? AccessKeyId { get; init; }

    /// <summary>Why the upload failed.</summary>
    [JsonPropertyName("error")]
    public string? Error { get; init; }
}
