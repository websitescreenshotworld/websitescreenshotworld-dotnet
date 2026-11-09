using WebsiteScreenshotWorld.Internal;

namespace WebsiteScreenshotWorld.AutoSchedule;

/// <summary>
/// Starts captures of web pages scheduled in the dashboard. Get it from
/// <see cref="WebsiteScreenshotWorldClient.AutoSchedule"/>.
/// </summary>
public sealed class AutoScheduleClient
{
    private readonly ApiTransport _transport;
    private readonly ClientSettings _settings;

    internal AutoScheduleClient(ApiTransport transport, ClientSettings settings)
    {
        _transport = transport;
        _settings = settings;
    }

    /// <summary>
    /// Queues a capture of a scheduled web page now. Calling again while that capture is pending starts nothing
    /// new and answers <see cref="TriggerStatus.AlreadyPending"/>, so the call is safe to repeat.
    /// </summary>
    /// <param name="webPageId">The web page id, shown in the dashboard and sent as <c>schedule_id</c> in webhooks.</param>
    /// <param name="requestOptions">A timeout for this call.</param>
    /// <param name="cancellationToken">Stops the call, including any retries.</param>
    /// <returns>Whether a capture was queued.</returns>
    /// <exception cref="InvalidOperationException">No Auto Schedule API key is set.</exception>
    /// <exception cref="NotFoundException">This account has no such web page.</exception>
    /// <exception cref="PlanLimitException">The plan has no trigger allowance left.</exception>
    public Task<TriggerResult> TriggerNowAsync(
        Guid webPageId,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        var url = new Uri($"{_settings.AutoScheduleBaseUrl}/auto-schedule/v1/web-pages/{webPageId:D}/trigger-now");
        var request = new ApiRequest(HttpMethod.Post, url, _settings.TimeoutFor(requestOptions, isCapture: false))
        {
            ApiKey = _settings.RequireAutoScheduleKey(),
        };
        return _transport.SendAsync(
            request,
            (response, token) => JsonBodies.ReadAsync(response, WswJsonContext.Default.TriggerResult, token),
            cancellationToken);
    }
}
