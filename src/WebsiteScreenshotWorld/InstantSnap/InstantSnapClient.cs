using System.Diagnostics;
using WebsiteScreenshotWorld.Internal;

namespace WebsiteScreenshotWorld.InstantSnap;

/// <summary>
/// Captures web pages on demand. Get it from <see cref="WebsiteScreenshotWorldClient.InstantSnap"/>.
/// </summary>
public sealed class InstantSnapClient
{
    private const string CapturesPath = "/instant-snap/v1/captures";

    private static readonly TimeSpan DefaultWaitTimeout = TimeSpan.FromSeconds(360);
    private static readonly TimeSpan DefaultPollInterval = TimeSpan.FromSeconds(3);

    private readonly ApiTransport _transport;
    private readonly ClientSettings _settings;

    internal InstantSnapClient(ApiTransport transport, ClientSettings settings)
    {
        _transport = transport;
        _settings = settings;
    }

    /// <summary>
    /// Captures a page and returns its metadata with a <see cref="Capture.DownloadUrl"/> valid for 60 minutes.
    /// </summary>
    /// <param name="options">What to capture. <see cref="CaptureOptions.Url"/> is required.</param>
    /// <param name="requestOptions">An idempotency key or timeout for this call.</param>
    /// <param name="cancellationToken">Stops the call, including any retries.</param>
    /// <returns>The capture.</returns>
    /// <exception cref="InvalidOperationException">No Instant Snap API key is set.</exception>
    /// <exception cref="WebsiteScreenshotWorldException">The API refused the capture or could not be reached.</exception>
    public Task<Capture> CaptureAsync(
        CaptureOptions options,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        var request = CaptureRequest(options, ResponseType.Json, isAsync: false, requestOptions);
        return _transport.SendAsync(request, ReadCaptureAsync, cancellationToken);
    }

    /// <summary>
    /// Captures a page and streams the file back in the response. The API does not keep the file, so dispose the
    /// result once it is read or saved.
    /// </summary>
    /// <param name="options">What to capture. <see cref="CaptureOptions.Url"/> is required.</param>
    /// <param name="requestOptions">An idempotency key or timeout for this call.</param>
    /// <param name="cancellationToken">Stops the call, including any retries.</param>
    /// <returns>The file and its metadata.</returns>
    /// <exception cref="InvalidOperationException">No Instant Snap API key is set.</exception>
    /// <exception cref="WebsiteScreenshotWorldException">The API refused the capture or could not be reached.</exception>
    public Task<CaptureFile> CaptureFileAsync(
        CaptureOptions options,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        var request = CaptureRequest(options, ResponseType.ByFormat, isAsync: false, requestOptions);
        return _transport.SendAsync(request, CaptureFile.ReadAsync, cancellationToken);
    }

    /// <summary>Captures a page and uploads the file to the bucket in <see cref="CaptureOptions.S3Delivery"/>.</summary>
    /// <param name="options">What to capture. <see cref="CaptureOptions.Url"/> and <see cref="CaptureOptions.S3Delivery"/> are required.</param>
    /// <param name="requestOptions">An idempotency key or timeout for this call.</param>
    /// <param name="cancellationToken">Stops the call, including any retries.</param>
    /// <returns>The capture, with the upload's progress in <see cref="Capture.S3Delivery"/>.</returns>
    /// <exception cref="InvalidOperationException">No Instant Snap API key is set.</exception>
    /// <exception cref="WebsiteScreenshotWorldException">The API refused the capture or could not be reached.</exception>
    public Task<Capture> CaptureToBucketAsync(
        CaptureOptions options,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        RequireBucket(options);
        var request = CaptureRequest(options, ResponseType.CloudStorage, isAsync: false, requestOptions);
        return _transport.SendAsync(request, ReadCaptureAsync, cancellationToken);
    }

    /// <summary>
    /// Queues an async capture and returns at once with the capture in <see cref="CaptureStatus.Queued"/>. The
    /// capture runs when a browser is free; follow it with <see cref="WaitAsync(Guid, TimeSpan?, TimeSpan?, RequestOptions?, CancellationToken)"/>,
    /// <see cref="GetAsync"/> or a webhook. A capture still waiting 15 minutes after it was accepted fails with
    /// <c>capture_expired</c> and is not charged.
    /// </summary>
    /// <param name="options">
    /// What to capture. <see cref="CaptureOptions.Url"/> is required; <see cref="CaptureOptions.TimeoutSeconds"/>
    /// sets the browser time the capture may use.
    /// </param>
    /// <param name="requestOptions">An idempotency key or timeout for this call.</param>
    /// <param name="cancellationToken">Stops the call, including any retries.</param>
    /// <returns>The capture, queued.</returns>
    /// <exception cref="InvalidOperationException">No Instant Snap API key is set.</exception>
    /// <exception cref="RateLimitException">
    /// <c>queue_full</c>: too many async captures are already waiting or running. Not retried; wait
    /// <see cref="WebsiteScreenshotWorldException.RetryAfter"/> before submitting again.
    /// </exception>
    /// <exception cref="WebsiteScreenshotWorldException">The API refused the capture or could not be reached.</exception>
    public Task<Capture> SubmitAsync(
        CaptureOptions options,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        var request = CaptureRequest(options, ResponseType.Json, isAsync: true, requestOptions);
        return _transport.SendAsync(request, ReadCaptureAsync, cancellationToken);
    }

    /// <summary>
    /// Queues an async capture that uploads the file to the bucket in <see cref="CaptureOptions.S3Delivery"/>, and
    /// returns at once with the capture in <see cref="CaptureStatus.Queued"/>.
    /// </summary>
    /// <param name="options">
    /// What to capture. <see cref="CaptureOptions.Url"/> and <see cref="CaptureOptions.S3Delivery"/> are required;
    /// <see cref="CaptureOptions.TimeoutSeconds"/> sets the browser time the capture may use.
    /// </param>
    /// <param name="requestOptions">An idempotency key or timeout for this call.</param>
    /// <param name="cancellationToken">Stops the call, including any retries.</param>
    /// <returns>The capture, queued.</returns>
    /// <exception cref="InvalidOperationException">No Instant Snap API key is set.</exception>
    /// <exception cref="RateLimitException">
    /// <c>queue_full</c>: too many async captures are already waiting or running. Not retried; wait
    /// <see cref="WebsiteScreenshotWorldException.RetryAfter"/> before submitting again.
    /// </exception>
    /// <exception cref="WebsiteScreenshotWorldException">The API refused the capture or could not be reached.</exception>
    public Task<Capture> SubmitToBucketAsync(
        CaptureOptions options,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        RequireBucket(options);
        var request = CaptureRequest(options, ResponseType.CloudStorage, isAsync: true, requestOptions);
        return _transport.SendAsync(request, ReadCaptureAsync, cancellationToken);
    }

    /// <summary>
    /// Reads a capture with <see cref="GetAsync"/> every <paramref name="pollInterval"/> until it is no longer
    /// queued, running or pending, and returns it.
    /// </summary>
    /// <remarks>
    /// When <paramref name="timeout"/> runs out first, the capture is returned as last read, still unfinished,
    /// rather than an exception thrown, so check <see cref="Capture.Status"/> and wait again if you like.
    /// </remarks>
    /// <param name="id">The capture id, as returned by <see cref="SubmitAsync"/> or <see cref="SubmitToBucketAsync"/>.</param>
    /// <param name="timeout">How long to keep reading. Defaults to 360 seconds; <see cref="Timeout.InfiniteTimeSpan"/> waits until it finishes.</param>
    /// <param name="pollInterval">The pause between reads. Defaults to 3 seconds; <see cref="TimeSpan.Zero"/> reads again at once.</param>
    /// <param name="requestOptions">A timeout for each read.</param>
    /// <param name="cancellationToken">Stops waiting at once.</param>
    /// <returns>The finished capture, or the last one read when <paramref name="timeout"/> ran out.</returns>
    /// <exception cref="InvalidOperationException">No Instant Snap API key is set.</exception>
    /// <exception cref="NotFoundException">This account has no such capture.</exception>
    /// <exception cref="WebsiteScreenshotWorldException">A read was refused or could not be sent, as from <see cref="GetAsync"/>.</exception>
    public async Task<Capture> WaitAsync(
        Guid id,
        TimeSpan? timeout = null,
        TimeSpan? pollInterval = null,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        var limit = timeout ?? DefaultWaitTimeout;
        if (limit < TimeSpan.Zero && limit != Timeout.InfiniteTimeSpan)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout), limit, "The wait cannot be negative.");
        }

        var interval = pollInterval ?? DefaultPollInterval;
        if (interval < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(pollInterval), interval, "The poll interval cannot be negative.");
        }

        var elapsed = Stopwatch.StartNew();
        while (true)
        {
            var capture = await GetAsync(id, requestOptions, cancellationToken).ConfigureAwait(false);
            if (!IsUnfinished(capture.Status))
            {
                return capture;
            }

            var pause = interval;
            if (limit != Timeout.InfiniteTimeSpan)
            {
                var remaining = limit - elapsed.Elapsed;
                if (remaining <= TimeSpan.Zero)
                {
                    return capture;
                }

                pause = remaining < pause ? remaining : pause;
            }

            if (pause > TimeSpan.Zero)
            {
                await Task.Delay(pause, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Reads <paramref name="capture"/> again every <paramref name="pollInterval"/> until it is no longer queued,
    /// running or pending, and returns it. See <see cref="WaitAsync(Guid, TimeSpan?, TimeSpan?, RequestOptions?, CancellationToken)"/>.
    /// </summary>
    /// <param name="capture">A capture, such as the one <see cref="SubmitAsync"/> returned.</param>
    /// <param name="timeout">How long to keep reading. Defaults to 360 seconds.</param>
    /// <param name="pollInterval">The pause between reads. Defaults to 3 seconds.</param>
    /// <param name="requestOptions">A timeout for each read.</param>
    /// <param name="cancellationToken">Stops waiting at once.</param>
    /// <returns>The finished capture, or the last one read when <paramref name="timeout"/> ran out.</returns>
    public Task<Capture> WaitAsync(
        Capture capture,
        TimeSpan? timeout = null,
        TimeSpan? pollInterval = null,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        Guard.NotNull(capture, nameof(capture));
        return WaitAsync(capture.Id, timeout, pollInterval, requestOptions, cancellationToken);
    }

    /// <summary>Reads a capture back, with a freshly issued <see cref="Capture.DownloadUrl"/>.</summary>
    /// <param name="id">The capture id.</param>
    /// <param name="requestOptions">A timeout for this call.</param>
    /// <param name="cancellationToken">Stops the call, including any retries.</param>
    /// <returns>The capture.</returns>
    /// <exception cref="InvalidOperationException">No Instant Snap API key is set.</exception>
    /// <exception cref="NotFoundException">This account has no such capture.</exception>
    public Task<Capture> GetAsync(
        Guid id,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        var request = new ApiRequest(HttpMethod.Get, Url($"{CapturesPath}/{id:D}"), _settings.TimeoutFor(requestOptions, isCapture: false))
        {
            ApiKey = _settings.RequireInstantSnapKey(),
        };
        return _transport.SendAsync(request, ReadCaptureAsync, cancellationToken);
    }

    /// <summary>Downloads a capture's file to <paramref name="path"/>, replacing any file already there.</summary>
    /// <param name="capture">A capture with a <see cref="Capture.DownloadUrl"/>.</param>
    /// <param name="path">The file to write.</param>
    /// <param name="requestOptions">A timeout for this call, covering the whole download.</param>
    /// <param name="cancellationToken">Stops the download.</param>
    /// <returns>A task that completes when the file is written.</returns>
    /// <exception cref="InvalidOperationException">The capture has no download link.</exception>
    public Task DownloadAsync(
        Capture capture,
        string path,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentException("A file path is required.", nameof(path));
        }

        var request = DownloadRequest(capture, requestOptions);
        return FileTarget.WriteAsync(path, (file, token) => DownloadAsync(request, file, token), cancellationToken);
    }

    /// <summary>Downloads a capture's file into <paramref name="destination"/>.</summary>
    /// <param name="capture">A capture with a <see cref="Capture.DownloadUrl"/>.</param>
    /// <param name="destination">A writable stream. It is left open.</param>
    /// <param name="requestOptions">A timeout for this call, covering the whole download.</param>
    /// <param name="cancellationToken">Stops the download.</param>
    /// <returns>A task that completes when the file is copied.</returns>
    /// <exception cref="InvalidOperationException">The capture has no download link.</exception>
    public Task DownloadAsync(
        Capture capture,
        Stream destination,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        Guard.NotNull(destination, nameof(destination));
        return DownloadAsync(DownloadRequest(capture, requestOptions), destination, cancellationToken);
    }

    /// <summary>
    /// Reads the values captures take for options they do not set, and the limits options are checked
    /// against. Needs no API key.
    /// </summary>
    /// <param name="requestOptions">A timeout for this call.</param>
    /// <param name="cancellationToken">Stops the call, including any retries.</param>
    /// <returns>The current default set.</returns>
    public Task<CaptureDefaults> GetDefaultsAsync(
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        var request = new ApiRequest(HttpMethod.Get, Url("/instant-snap/v1/defaults"), _settings.TimeoutFor(requestOptions, isCapture: false));
        return _transport.SendAsync(request, ReadDefaultsAsync, cancellationToken);
    }

    /// <summary>Lists the devices and proxy countries captures accept.</summary>
    /// <param name="requestOptions">A timeout for this call.</param>
    /// <param name="cancellationToken">Stops the call, including any retries.</param>
    /// <returns>The catalog.</returns>
    /// <exception cref="InvalidOperationException">No Instant Snap API key is set.</exception>
    public Task<CaptureCatalog> GetCatalogAsync(
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        var request = new ApiRequest(HttpMethod.Get, Url("/instant-snap/v1/catalog"), _settings.TimeoutFor(requestOptions, isCapture: false))
        {
            ApiKey = _settings.RequireInstantSnapKey(),
        };
        return _transport.SendAsync(
            request,
            (response, token) => JsonBodies.ReadAsync(response, WswJsonContext.Default.CaptureCatalog, token),
            cancellationToken);
    }

    private static Task<Capture> ReadCaptureAsync(HttpResponseMessage response, CancellationToken cancellationToken) =>
        JsonBodies.ReadAsync(response, WswJsonContext.Default.Capture, cancellationToken);

    private static async Task<CaptureDefaults> ReadDefaultsAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var defaults = await JsonBodies.ReadAsync(response, WswJsonContext.Default.CaptureDefaults, cancellationToken).ConfigureAwait(false);
        defaults.AdditionalProperties?.Remove("success");
        return defaults;
    }

    private static bool IsUnfinished(CaptureStatus status) =>
        status == CaptureStatus.Queued || status == CaptureStatus.Running || status == CaptureStatus.Pending;

    private static void RequireBucket(CaptureOptions options)
    {
        Guard.NotNull(options, nameof(options));
        if (options.S3Delivery is null)
        {
            throw new ArgumentException($"{nameof(CaptureOptions.S3Delivery)} is required to capture to a bucket.", nameof(options));
        }
    }

    private ApiRequest CaptureRequest(CaptureOptions options, ResponseType responseType, bool isAsync, RequestOptions? requestOptions)
    {
        Guard.NotNull(options, nameof(options));
        if (string.IsNullOrWhiteSpace(options.Url))
        {
            throw new ArgumentException($"{nameof(CaptureOptions.Url)} is required.", nameof(options));
        }

        // An async capture is only queued while the caller waits, so it takes the timeout for everything else.
        return new ApiRequest(HttpMethod.Post, Url(CapturesPath), _settings.TimeoutFor(requestOptions, isCapture: !isAsync))
        {
            ApiKey = _settings.RequireInstantSnapKey(),
            JsonBody = JsonBodies.CaptureRequest(options, responseType, isAsync),
            IdempotencyKey = requestOptions?.IdempotencyKey ?? Guid.NewGuid().ToString(),
            ExpectsJson = responseType != ResponseType.ByFormat,
        };
    }

    private ApiRequest DownloadRequest(Capture capture, RequestOptions? requestOptions)
    {
        Guard.NotNull(capture, nameof(capture));
        if (capture.DownloadUrl is null)
        {
            throw new InvalidOperationException(
                $"Capture {capture.Id} has no download link. Files returned in the response or uploaded to a bucket have none; " +
                $"for others, call {nameof(GetAsync)} for a fresh link before the file expires.");
        }

        // The link is signed, so the API key is not sent to the file host.
        return new ApiRequest(HttpMethod.Get, capture.DownloadUrl, _settings.TimeoutFor(requestOptions, isCapture: false))
        {
            ExpectsJson = false,
        };
    }

    private async Task DownloadAsync(ApiRequest request, Stream destination, CancellationToken cancellationToken)
    {
        await _transport.SendAsync(
            request,
            async (response, token) =>
            {
                using (response)
                {
                    var content = await response.Content.ReadAsStreamCompatAsync(token).ConfigureAwait(false);
                    await content.CopyToAsync(destination, FileTarget.BufferSize, token).ConfigureAwait(false);
                    return destination;
                }
            },
            cancellationToken).ConfigureAwait(false);
    }

    private Uri Url(string path) => new(_settings.InstantSnapBaseUrl + path);
}
