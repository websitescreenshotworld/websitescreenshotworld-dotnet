using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using WebsiteScreenshotWorld.Tests.Support;
using WebsiteScreenshotWorld.Webhooks;

namespace WebsiteScreenshotWorld.Tests;

/// <summary>Every example in <c>spec/events</c> reads into the right typed event with every field mapped.</summary>
public sealed class WebhookEventTests
{
    private const string Secret = "whsec_test";
    private static readonly DateTimeOffset Now = DateTimeOffset.FromUnixTimeSeconds(1790418600);

    public static TheoryData<string, Type> Events() => new()
    {
        { "auto-schedule.capture.changed.json", typeof(CaptureChangedEvent) },
        { "auto-schedule.capture.failed.json", typeof(ScheduledCaptureFailedEvent) },
        { "auto-schedule.capture.succeeded.json", typeof(ScheduledCaptureSucceededEvent) },
        { "auto-schedule.delivery.completed.json", typeof(DeliveryCompletedEvent) },
        { "auto-schedule.ping.json", typeof(PingEvent) },
        { "auto-schedule.schedule.paused.json", typeof(SchedulePausedEvent) },
        { "auto-schedule.sitemap_run.completed.json", typeof(SitemapRunCompletedEvent) },
        { "instant-snap.capture.failed.json", typeof(InstantSnapCaptureFailedEvent) },
        { "instant-snap.capture.succeeded.json", typeof(InstantSnapCaptureSucceededEvent) },
        { "unknown.json", typeof(UnknownWebhookEvent) },
    };

    [Fact]
    public void EveryExampleIsCovered()
    {
        var files = Directory.GetFiles(RepoPaths.Spec("events"), "*.json").Select(Path.GetFileName).Order(StringComparer.Ordinal);
        var covered = Events().Select(row => (string)row[0]).Order(StringComparer.Ordinal);
        Assert.Equal(files, covered);
    }

    [Theory]
    [MemberData(nameof(Events))]
    public void ExampleReadsIntoTypedEvent(string fileName, Type expectedType)
    {
        var body = File.ReadAllText(RepoPaths.Spec("events", fileName));
        using var document = JsonDocument.Parse(body);
        var envelope = document.RootElement;

        var webhookEvent = Webhook.Verify(body, Sign(body), Secret, now: Now);

        Assert.IsType(expectedType, webhookEvent);
        Assert.Equal(envelope.GetProperty("id").GetString(), webhookEvent.Id);
        Assert.Equal(envelope.GetProperty("type").GetString(), webhookEvent.Type);
        Assert.Equal(envelope.GetProperty("product").GetString(), webhookEvent.Product.ToString());
        Assert.Equal(DateTimeOffset.Parse("2026-09-26T10:30:05Z", CultureInfo.InvariantCulture), webhookEvent.CreatedAt);

        var data = webhookEvent switch
        {
            InstantSnapCaptureSucceededEvent e => JsonAssert.ToWire(e.Data),
            InstantSnapCaptureFailedEvent e => JsonAssert.ToWire(e.Data),
            ScheduledCaptureSucceededEvent e => JsonAssert.ToWire(e.Data),
            ScheduledCaptureFailedEvent e => JsonAssert.ToWire(e.Data),
            DeliveryCompletedEvent e => JsonAssert.ToWire(e.Data),
            CaptureChangedEvent e => JsonAssert.ToWire(e.Data),
            SchedulePausedEvent e => JsonAssert.ToWire(e.Data),
            SitemapRunCompletedEvent e => JsonAssert.ToWire(e.Data),
            PingEvent e => JsonAssert.ToWire(e.Data),
            UnknownWebhookEvent e => e.Data,
            _ => throw new InvalidOperationException(webhookEvent.GetType().Name),
        };
        JsonAssert.Subset(envelope.GetProperty("data"), data);
    }

    [Fact]
    public void SignedBodyThatIsNotAnEventIsRefused()
    {
        const string body = "[1, 2, 3]";

        var exception = Assert.Throws<WebsiteScreenshotWorldException>(() => Webhook.Verify(body, Sign(body), Secret, now: Now));

        Assert.IsNotType<SignatureVerificationException>(exception);
    }

    private static string Sign(string body)
    {
        var timestamp = Now.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);
        var digest = HMACSHA256.HashData(Encoding.UTF8.GetBytes(Secret), Encoding.UTF8.GetBytes(timestamp + "." + body));
        return $"t={timestamp},v1={Convert.ToHexString(digest).ToLowerInvariant()}";
    }
}
