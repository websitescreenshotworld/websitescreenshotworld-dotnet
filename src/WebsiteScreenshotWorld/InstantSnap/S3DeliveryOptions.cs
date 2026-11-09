using System.Text.Json.Serialization;

namespace WebsiteScreenshotWorld.InstantSnap;

/// <summary>
/// An S3-compatible bucket to upload the capture to. Set <see cref="Region"/> or <see cref="EndpointUrl"/>.
/// </summary>
public sealed class S3DeliveryOptions
{
    /// <summary>The access key id. Required.</summary>
    [JsonPropertyName("access_key_id")]
    public string? AccessKeyId { get; set; }

    /// <summary>The secret access key. Required, and never returned by the API.</summary>
    [JsonPropertyName("secret_access_key")]
    public string? SecretAccessKey { get; set; }

    /// <summary>The bucket name. Required.</summary>
    [JsonPropertyName("bucket")]
    public string? Bucket { get; set; }

    /// <summary>The bucket's region, such as <c>eu-west-2</c>.</summary>
    [JsonPropertyName("region")]
    public string? Region { get; set; }

    /// <summary>The storage endpoint for a provider other than AWS. https only, and not a private address.</summary>
    [JsonPropertyName("endpoint_url")]
    public string? EndpointUrl { get; set; }

    /// <summary>Address the bucket in the path rather than the host name. Defaults to <see langword="false"/>.</summary>
    [JsonPropertyName("force_path_style")]
    public bool? ForcePathStyle { get; set; }

    /// <summary>A prefix for the object key, such as <c>captures/</c>.</summary>
    [JsonPropertyName("key_prefix")]
    public string? KeyPrefix { get; set; }

    /// <summary>
    /// Wait up to 30 seconds for the upload to finish before answering. Defaults to <see langword="false"/>.
    /// </summary>
    [JsonPropertyName("wait_for_sync")]
    public bool? WaitForSync { get; set; }
}
