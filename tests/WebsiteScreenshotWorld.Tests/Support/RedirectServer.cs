using System.Net;
using System.Net.Sockets;

namespace WebsiteScreenshotWorld.Tests.Support;

/// <summary>
/// A local server that answers every request with a redirect to <c>/elsewhere</c> on itself, and counts the
/// requests that arrive there.
/// </summary>
internal sealed class RedirectServer : IDisposable
{
    private readonly HttpListener _listener = new();
    private readonly Task _loop;
    private int _elsewhere;

    public RedirectServer()
    {
        var port = FreePort();
        BaseUrl = new Uri($"http://127.0.0.1:{port}");
        _listener.Prefixes.Add($"{BaseUrl}");
        _listener.Start();
        _loop = Task.Run(ServeAsync);
    }

    public Uri BaseUrl { get; }

    /// <summary>How many requests followed the redirect.</summary>
    public int Elsewhere => Volatile.Read(ref _elsewhere);

    /// <summary>The API key a followed redirect carried, if one did.</summary>
    public string? KeySentElsewhere { get; private set; }

    public void Dispose()
    {
        _listener.Stop();
        _listener.Close();
        try
        {
            _loop.Wait(TimeSpan.FromSeconds(5));
        }
        catch (AggregateException)
        {
        }
    }

    private async Task ServeAsync()
    {
        while (_listener.IsListening)
        {
            HttpListenerContext context;
            try
            {
                context = await _listener.GetContextAsync();
            }
            catch (Exception exception) when (exception is HttpListenerException or ObjectDisposedException or InvalidOperationException)
            {
                return;
            }

            if (context.Request.Url!.AbsolutePath == "/elsewhere")
            {
                Interlocked.Increment(ref _elsewhere);
                KeySentElsewhere = context.Request.Headers["X-Api-Key"];
                context.Response.StatusCode = 200;
                context.Response.ContentType = "application/json";
                await using (var body = context.Response.OutputStream)
                {
                    await body.WriteAsync("{}"u8.ToArray());
                }
            }
            else
            {
                context.Response.StatusCode = 302;
                context.Response.RedirectLocation = $"{BaseUrl}elsewhere";
                context.Response.Close();
            }
        }
    }

    private static int FreePort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}
