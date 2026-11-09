using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace WebsiteScreenshotWorld.Tests.Support;

/// <summary>Runs <c>conformance/server.js</c> for the duration of a test class.</summary>
public sealed class ConformanceServer : IAsyncLifetime
{
    private readonly HttpClient _http = new();
    private Process? _process;

    public Uri BaseUrl { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var start = new ProcessStartInfo("node")
        {
            RedirectStandardOutput = true,
            UseShellExecute = false,
        };
        start.ArgumentList.Add(RepoPaths.ConformanceServer);
        start.Environment.Remove("PORT");

        _process = Process.Start(start) ?? throw new InvalidOperationException("Could not start node.");
        var line = await _process.StandardOutput.ReadLineAsync().WaitAsync(TimeSpan.FromSeconds(30));
        var match = Regex.Match(line ?? string.Empty, @"http://127\.0\.0\.1:(\d+)");
        if (!match.Success)
        {
            throw new InvalidOperationException($"Unexpected first line from the conformance server: {line}");
        }

        BaseUrl = new Uri($"http://127.0.0.1:{match.Groups[1].Value}");
    }

    public Task DisposeAsync()
    {
        _http.Dispose();
        if (_process is { HasExited: false })
        {
            _process.Kill();
            _process.WaitForExit();
        }

        _process?.Dispose();
        return Task.CompletedTask;
    }

    public async Task LoadAsync(JsonElement fixture)
    {
        using var response = await _http.PostAsJsonAsync(new Uri(BaseUrl, "/__load"), fixture);
        response.EnsureSuccessStatusCode();
    }

    public async Task<JsonElement> RequestsAsync() =>
        await _http.GetFromJsonAsync<JsonElement>(new Uri(BaseUrl, "/__requests"));
}
