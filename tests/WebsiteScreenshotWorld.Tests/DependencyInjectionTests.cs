using Microsoft.Extensions.DependencyInjection;
using WebsiteScreenshotWorld.Tests.Support;

namespace WebsiteScreenshotWorld.Tests;

public sealed class DependencyInjectionTests
{
    [Fact]
    public async Task RegistersTypedClientWithConfiguredOptions()
    {
        var handler = StubHandler.Json(
            """{"success":true,"status":"triggered","web_page_id":"7d4e2b1a-6c5f-4a3e-9d8c-2b1a0f9e8d7c","triggered_by":"api","triggered_at":"2026-09-26T10:30:00.000Z","message":"Queued."}""");
        var services = new ServiceCollection();
        services
            .AddWebsiteScreenshotWorld(options =>
            {
                options.AutoScheduleApiKey = "wsw_autoapi_from_di";
                options.AutoScheduleBaseUrl = new Uri("https://autoapi.example.test");
            })
            .ConfigurePrimaryHttpMessageHandler(() => handler);
        using var provider = services.BuildServiceProvider();

        var client = provider.GetRequiredService<WebsiteScreenshotWorldClient>();
        await client.AutoSchedule.TriggerNowAsync(Guid.Parse("7d4e2b1a-6c5f-4a3e-9d8c-2b1a0f9e8d7c"));

        var request = handler.Requests.Single();
        Assert.Equal("wsw_autoapi_from_di", request.Headers["X-Api-Key"]);
        Assert.Equal("autoapi.example.test", request.Uri.Host);
    }
}
