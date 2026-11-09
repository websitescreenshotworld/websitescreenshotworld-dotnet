using System.Net.Http.Headers;

namespace WebsiteScreenshotWorld.Internal;

/// <summary>Everything needed to send one call, possibly more than once.</summary>
internal sealed class ApiRequest
{
    private const string JsonMediaType = "application/json";

    public ApiRequest(HttpMethod method, Uri uri, TimeSpan timeout)
    {
        Method = method;
        Uri = uri;
        Timeout = timeout;
    }

    public HttpMethod Method { get; }

    public Uri Uri { get; }

    public TimeSpan Timeout { get; }

    public string? ApiKey { get; init; }

    public byte[]? JsonBody { get; init; }

    public string? IdempotencyKey { get; init; }

    public bool ExpectsJson { get; init; } = true;

    public HttpRequestMessage CreateMessage()
    {
        var message = new HttpRequestMessage(Method, Uri);
        message.Headers.TryAddWithoutValidation("User-Agent", UserAgent.Value);

        if (ExpectsJson)
        {
            message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(JsonMediaType));
        }

        if (ApiKey is not null)
        {
            message.Headers.TryAddWithoutValidation("X-Api-Key", ApiKey);
        }

        if (IdempotencyKey is not null)
        {
            message.Headers.TryAddWithoutValidation("Idempotency-Key", IdempotencyKey);
        }

        if (JsonBody is not null)
        {
            message.Content = new ByteArrayContent(JsonBody);
            message.Content.Headers.ContentType = new MediaTypeHeaderValue(JsonMediaType);
        }

        return message;
    }
}
