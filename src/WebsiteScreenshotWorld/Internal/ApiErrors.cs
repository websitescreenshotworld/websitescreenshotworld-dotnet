using System.Net;
using System.Text.Json;

namespace WebsiteScreenshotWorld.Internal;

/// <summary>Turns a refusal into the matching exception, following <c>spec/errors.json</c>.</summary>
internal static class ApiErrors
{
    private const string UnknownError = "unknown_error";

    public static bool IsRetryable(string? errorCode) => errorCode is
        "rate_limited" or
        "capture_busy" or
        "service_unavailable" or
        "idempotency_key_in_use";

    public static async Task<WebsiteScreenshotWorldException> ReadAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var body = await response.Content.ReadAsStringCompatAsync(cancellationToken).ConfigureAwait(false);
        var status = response.StatusCode;
        var headerRetryAfter = RetryAfterHeader(response);

        if (TryReadRefusal(body, out var code, out var message, out var retryAfter))
        {
            return Create(
                code,
                message ?? DefaultMessage(response),
                status,
                retryAfter ?? headerRetryAfter,
                body);
        }

        return Create(FallbackCode(status), DefaultMessage(response), status, headerRetryAfter, body);
    }

    public static WebsiteScreenshotWorldException Create(
        string code,
        string message,
        HttpStatusCode status,
        TimeSpan? retryAfter,
        string? body) => code switch
        {
            "api_key_invalid" =>
                new AuthenticationException(message, code, status, retryAfter, body),
            "validation_failed" or "s3_destination_invalid" or "web_page_id_invalid" or "idempotency_key_reused"
                or "sitemap_schedule_not_triggerable" =>
                new InvalidRequestException(message, code, status, retryAfter, body),
            "quota_exhausted" or "feature_not_available" or "no_trigger_allowance" or "forbidden" =>
                new PlanLimitException(message, code, status, retryAfter, body),
            "capture_not_found" or "web_page_not_found" or "not_found" =>
                new NotFoundException(message, code, status, retryAfter, body),
            "rate_limited" or "queue_full" =>
                new RateLimitException(message, code, status, retryAfter, body),
            "capture_failed" or "capture_timeout" or "payload_too_large" or "capture_expired" =>
                new CaptureException(message, code, status, retryAfter, body),
            "capture_busy" or "service_unavailable" or "internal_error" or "request_cancelled" or "idempotency_key_in_use" =>
                new ServiceException(message, code, status, retryAfter, body),
            _ =>
                new WebsiteScreenshotWorldException(message, code, status, retryAfter, body),
        };

    private static string FallbackCode(HttpStatusCode status) => (int)status switch
    {
        400 => "validation_failed",
        401 => "api_key_invalid",
        403 => "forbidden",
        404 => "not_found",
        409 => "idempotency_key_in_use",
        413 => "payload_too_large",
        429 => "rate_limited",
        500 => "internal_error",
        502 or 503 or 504 => "service_unavailable",
        _ => UnknownError,
    };

    private static bool TryReadRefusal(string body, out string code, out string? message, out TimeSpan? retryAfter)
    {
        code = UnknownError;
        message = null;
        retryAfter = null;

        if (string.IsNullOrWhiteSpace(body))
        {
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object ||
                !root.TryGetProperty("error_code", out var codeElement) ||
                codeElement.ValueKind != JsonValueKind.String)
            {
                return false;
            }

            code = codeElement.GetString()!;
            if (root.TryGetProperty("error_message", out var messageElement) && messageElement.ValueKind == JsonValueKind.String)
            {
                message = messageElement.GetString();
            }

            if (root.TryGetProperty("retry_after_seconds", out var retryElement) &&
                retryElement.ValueKind == JsonValueKind.Number &&
                retryElement.TryGetDouble(out var seconds) &&
                seconds >= 0)
            {
                retryAfter = TimeSpan.FromSeconds(seconds);
            }

            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static TimeSpan? RetryAfterHeader(HttpResponseMessage response)
    {
        var header = response.Headers.RetryAfter;
        if (header?.Delta is { } delta)
        {
            return delta;
        }

        if (header?.Date is { } date)
        {
            var wait = date - DateTimeOffset.UtcNow;
            return wait > TimeSpan.Zero ? wait : TimeSpan.Zero;
        }

        return null;
    }

    private static string DefaultMessage(HttpResponseMessage response)
    {
        var status = (int)response.StatusCode;
        return string.IsNullOrEmpty(response.ReasonPhrase)
            ? $"The API answered with HTTP {status}."
            : $"The API answered with HTTP {status} ({response.ReasonPhrase}).";
    }
}
