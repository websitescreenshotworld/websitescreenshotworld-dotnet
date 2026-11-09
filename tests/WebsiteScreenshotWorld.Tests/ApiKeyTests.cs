using WebsiteScreenshotWorld.InstantSnap;
using WebsiteScreenshotWorld.Tests.Support;

namespace WebsiteScreenshotWorld.Tests;

/// <summary>Reads and changes the key environment variables, so its tests do not run in parallel with each other.</summary>
public sealed class ApiKeyTests : IDisposable
{
    private readonly string? _instantSnapKey = Environment.GetEnvironmentVariable(WebsiteScreenshotWorldClientOptions.InstantSnapApiKeyVariable);
    private readonly string? _autoScheduleKey = Environment.GetEnvironmentVariable(WebsiteScreenshotWorldClientOptions.AutoScheduleApiKeyVariable);

    public ApiKeyTests()
    {
        Environment.SetEnvironmentVariable(WebsiteScreenshotWorldClientOptions.InstantSnapApiKeyVariable, null);
        Environment.SetEnvironmentVariable(WebsiteScreenshotWorldClientOptions.AutoScheduleApiKeyVariable, null);
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable(WebsiteScreenshotWorldClientOptions.InstantSnapApiKeyVariable, _instantSnapKey);
        Environment.SetEnvironmentVariable(WebsiteScreenshotWorldClientOptions.AutoScheduleApiKeyVariable, _autoScheduleKey);
    }

    [Fact]
    public async Task MissingInstantSnapKeyFailsBeforeSending()
    {
        var handler = StubHandler.Json("{}");
        using var client = new WebsiteScreenshotWorldClient(new HttpClient(handler), new WebsiteScreenshotWorldClientOptions { AutoScheduleApiKey = "wsw_autoapi_test" });

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => client.InstantSnap.CaptureAsync(new CaptureOptions { Url = "https://example.com" }));

        Assert.Contains("InstantSnapApiKey", exception.Message, StringComparison.Ordinal);
        Assert.Contains("WSW_INSTANT_SNAP_API_KEY", exception.Message, StringComparison.Ordinal);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task MissingAutoScheduleKeyFailsBeforeSending()
    {
        var handler = StubHandler.Json("{}");
        using var client = new WebsiteScreenshotWorldClient(new HttpClient(handler), new WebsiteScreenshotWorldClientOptions { InstantSnapApiKey = "wsw_api_test" });

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => client.AutoSchedule.TriggerNowAsync(Guid.NewGuid()));

        Assert.Contains("WSW_AUTO_SCHEDULE_API_KEY", exception.Message, StringComparison.Ordinal);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task KeysDefaultFromEnvironment()
    {
        Environment.SetEnvironmentVariable(WebsiteScreenshotWorldClientOptions.InstantSnapApiKeyVariable, "wsw_api_from_env");
        var handler = StubHandler.Json("""{"success":true,"emulated_devices":[],"proxy_countries":[]}""");
        using var client = new WebsiteScreenshotWorldClient(new HttpClient(handler));

        await client.InstantSnap.GetCatalogAsync();

        Assert.Equal("wsw_api_from_env", handler.Requests.Single().Headers["X-Api-Key"]);
    }

    [Fact]
    public async Task DefaultsNeedNoKey()
    {
        var handler = StubHandler.Json("""{"success":true,"spec_version":"v1","zoom":100}""");
        using var client = new WebsiteScreenshotWorldClient(new HttpClient(handler));

        var defaults = await client.InstantSnap.GetDefaultsAsync();

        Assert.Equal("v1", defaults.SpecVersion);
        Assert.Equal(100, defaults.Zoom);
        Assert.False(handler.Requests.Single().Headers.ContainsKey("X-Api-Key"));
        Assert.False(defaults.AdditionalProperties?.ContainsKey("success") ?? false);
    }
}
