using System.Globalization;

namespace WebsiteScreenshotWorld.Internal;

/// <summary>Sends calls with a timeout for the whole call and the retry rules of the API.</summary>
internal sealed class ApiTransport
{
    private static readonly TimeSpan LongestRetryAfter = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan[] Backoff = [TimeSpan.FromMilliseconds(500), TimeSpan.FromSeconds(1)];
    private static readonly object JitterLock = new();
    private static readonly Random Jitter = new();

    private readonly HttpClient _httpClient;
    private readonly int _maxRetries;

    public ApiTransport(HttpClient httpClient, int maxRetries)
    {
        _httpClient = httpClient;
        _maxRetries = maxRetries;
    }

    /// <summary>
    /// Sends <paramref name="request"/> until it succeeds, fails for good, runs out of retries or runs out of time.
    /// <see cref="ApiRequest.Timeout"/> bounds the whole call: every attempt and the waits between them, until a
    /// successful answer's headers have arrived. On success, <paramref name="readSuccess"/> takes ownership of the
    /// response.
    /// </summary>
    public async Task<T> SendAsync<T>(
        ApiRequest request,
        Func<HttpResponseMessage, CancellationToken, Task<T>> readSuccess,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(request.Timeout);
        var expiresAt = DateTimeOffset.UtcNow + request.Timeout;

        for (var attempt = 0; ; attempt++)
        {
            var canRetry = attempt < _maxRetries;
            WebsiteScreenshotWorldException failure;

            using (var message = request.CreateMessage())
            {
                HttpResponseMessage? response = null;
                try
                {
                    response = await _httpClient
                        .SendAsync(message, HttpCompletionOption.ResponseHeadersRead, deadline.Token)
                        .ConfigureAwait(false);
                    RefuseRedirected(request, response);

                    if (response.IsSuccessStatusCode)
                    {
                        return await ReadSuccessAsync(request, response, readSuccess, deadline.Token, cancellationToken).ConfigureAwait(false);
                    }

                    using (response)
                    {
                        failure = await ApiErrors.ReadAsync(response, deadline.Token).ConfigureAwait(false);
                    }

                    if (!canRetry || !ApiErrors.IsRetryable(failure.ErrorCode))
                    {
                        throw failure;
                    }
                }
                catch (Exception exception) when (IsConnectionFailure(exception, cancellationToken))
                {
                    response?.Dispose();
                    failure = ConnectionFailed(request, exception);
                    if (!canRetry || exception is OperationCanceledException)
                    {
                        // Running out of time ends the call; it is not retried.
                        throw failure;
                    }
                }
            }

            var wait = failure.RetryAfter is { } hint
                ? (hint < LongestRetryAfter ? hint : LongestRetryAfter)
                : BackoffDelay(attempt);
            if (DateTimeOffset.UtcNow + wait >= expiresAt)
            {
                throw failure;
            }

            try
            {
                await Task.Delay(wait, deadline.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
            {
                throw ConnectionFailed(request, exception);
            }
        }
    }

    /// <summary>
    /// Refuses an answer that came from somewhere else. This client's own <see cref="HttpClient"/> never follows
    /// redirects; one passed in may, and would already have sent the API key along with it.
    /// </summary>
    private static void RefuseRedirected(ApiRequest request, HttpResponseMessage response)
    {
        var finalUri = response.RequestMessage?.RequestUri;
        if (finalUri is null || finalUri == request.Uri)
        {
            return;
        }

        response.Dispose();
        throw new WebsiteScreenshotWorldException(
            $"The answer came from {finalUri.Host} after a redirect. The HttpClient passed to WebsiteScreenshotWorldClient must not follow redirects: set AllowAutoRedirect = false on its handler.");
    }

    private static async Task<T> ReadSuccessAsync<T>(
        ApiRequest request,
        HttpResponseMessage response,
        Func<HttpResponseMessage, CancellationToken, Task<T>> readSuccess,
        CancellationToken attemptToken,
        CancellationToken cancellationToken)
    {
        try
        {
            return await readSuccess(response, attemptToken).ConfigureAwait(false);
        }
        catch (Exception exception) when (IsConnectionFailure(exception, cancellationToken))
        {
            response.Dispose();
            throw ConnectionFailed(request, exception);
        }
        catch
        {
            response.Dispose();
            throw;
        }
    }

    private static bool IsConnectionFailure(Exception exception, CancellationToken cancellationToken) =>
        exception is HttpRequestException or IOException ||
        (exception is OperationCanceledException && !cancellationToken.IsCancellationRequested);

    private static WebsiteScreenshotWorldConnectionException ConnectionFailed(ApiRequest request, Exception exception)
    {
        var host = request.Uri.Host;
        if (exception is OperationCanceledException)
        {
            var seconds = request.Timeout.TotalSeconds.ToString("0.###", CultureInfo.InvariantCulture);
            var message = $"No answer from {host} within {seconds} seconds.";
            return new WebsiteScreenshotWorldConnectionException(message, new TimeoutException(message, exception));
        }

        return new WebsiteScreenshotWorldConnectionException($"Could not reach {host}: {exception.Message}", exception);
    }

    private static TimeSpan BackoffDelay(int attempt)
    {
        var baseDelay = Backoff[Math.Min(attempt, Backoff.Length - 1)];
        double factor;
        lock (JitterLock)
        {
            factor = 1 + (Jitter.NextDouble() * 0.25);
        }

        return TimeSpan.FromMilliseconds(baseDelay.TotalMilliseconds * factor);
    }
}
