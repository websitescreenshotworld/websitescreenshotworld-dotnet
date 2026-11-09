using System.Text;
using System.Text.Json;
using WebsiteScreenshotWorld.Tests.Support;
using WebsiteScreenshotWorld.Webhooks;

namespace WebsiteScreenshotWorld.Tests;

/// <summary>Runs every case in <c>spec/webhook-vectors.json</c>.</summary>
public sealed class WebhookVectorTests
{
    private static readonly Lazy<JsonDocument> Vectors =
        new(() => JsonDocument.Parse(File.ReadAllText(RepoPaths.Spec("webhook-vectors.json"))));

    public static TheoryData<string> Cases() =>
        new(Vectors.Value.RootElement.GetProperty("cases").EnumerateArray().Select(c => c.GetProperty("name").GetString()!));

    [Theory]
    [MemberData(nameof(Cases))]
    public void StringBodyAgreesWithVector(string name)
    {
        var (vector, secret, now, tolerance) = Load(name);
        var body = vector.GetProperty("body").GetString()!;
        var header = vector.GetProperty("header").GetString()!;

        AssertOutcome(vector, () => Webhook.Verify(body, header, secret, tolerance, now));
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void ByteBodyAgreesWithVector(string name)
    {
        var (vector, secret, now, tolerance) = Load(name);
        var body = Encoding.UTF8.GetBytes(vector.GetProperty("body").GetString()!);
        var header = vector.GetProperty("header").GetString()!;

        AssertOutcome(vector, () => Webhook.Verify(body, header, secret, tolerance, now));
    }

    private static (JsonElement Vector, string Secret, DateTimeOffset Now, TimeSpan? Tolerance) Load(string name)
    {
        var root = Vectors.Value.RootElement;
        var vector = root.GetProperty("cases").EnumerateArray().Single(c => c.GetProperty("name").GetString() == name);
        var tolerance = vector.TryGetProperty("tolerance", out var seconds) ? TimeSpan.FromSeconds(seconds.GetInt32()) : (TimeSpan?)null;
        return (
            vector,
            root.GetProperty("secret").GetString()!,
            DateTimeOffset.FromUnixTimeSeconds(vector.GetProperty("now").GetInt64()),
            tolerance);
    }

    private static void AssertOutcome(JsonElement vector, Func<WebhookEvent> verify)
    {
        if (vector.GetProperty("valid").GetBoolean())
        {
            var webhookEvent = Assert.IsType<PingEvent>(verify());
            Assert.Equal("8c1f0a2e-4b6d-4f3a-9e21-5d7c9b0a1f11", webhookEvent.Id);
        }
        else
        {
            Assert.Throws<SignatureVerificationException>(verify);
        }
    }
}
