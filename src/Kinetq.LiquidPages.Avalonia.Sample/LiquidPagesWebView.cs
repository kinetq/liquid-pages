using Kinetq.LiquidPages.Builders;
using Kinetq.LiquidPages.Interfaces;
using Kinetq.LiquidPages.Models;
using Kinetq.LiquidPages.Router.Interfaces;
using System.Text;

namespace Kinetq.LiquidPages.Avalonia.Sample;

public class LiquidPagesWebView : WebView
{
    private bool _webViewReady;

    public IRouteTree? RouteTree { get; set; }

    public ILiquidResponseMiddleware? LiquidResponseMiddleware { get; set; }

    public ITemplateOptionsManager? TemplateOptionsManager { get; set; }

    public event EventHandler<LiquidPagesResponseEventArgs>? ResponseRendered;

    public LiquidPagesWebView()
    {
        Loaded += OnLoaded;
        Navigating += OnNavigating;
    }

    public Task NavigateToPathAsync(string? path)
    {
        return OpenPathAsync(path);
    }

    private async void OnLoaded(object? sender, EventArgs e)
    {
        _webViewReady = true;
        await OpenPathAsync("/");
    }

    private async void OnNavigating(object? sender, WebNavigatingEventArgs args)
    {
        if (!Uri.TryCreate(args.Url, UriKind.Absolute, out var uri))
        {
            return;
        }

        if (uri.Scheme == "data" || uri.Scheme == "about")
        {
            return;
        }

        args.Cancel = true;
        await OpenPathAsync(uri.AbsolutePath);
    }

    private async Task OpenPathAsync(string? path)
    {
        var normalizedPath = NormalizePath(path);

        var response = await HandlePathAsync(normalizedPath);
        Source = new HtmlWebViewSource
        {
            Html = response.Body
        };
        
        OnResponseRendered(normalizedPath, response);
    }

    private void OnResponseRendered(string path, RenderedResponse response)
    {
        ResponseRendered?.Invoke(this, new LiquidPagesResponseEventArgs(path, response.StatusCode, response.ContentType));
    }

    private async Task<RenderedResponse> HandlePathAsync(string path)
    {
        var routeMatch = RouteTree.Match(path);
        var request = new LiquidRequestModel
        {
            Route = path,
            Method = "GET",
            QueryParams = new Dictionary<string, string>(),
            LiquidRoute = routeMatch?.LiquidRoute,
            RouteValues = routeMatch?.RouteValues ?? EmptyRouteValuesDictionary.Instance
        };

        await using var memoryStream = new MemoryStream();
        await using var writer = new StreamWriter(memoryStream, Encoding.UTF8, leaveOpen: true);

        var contentType = "text/html";
        var statusCode = 200;

        var response = new LiquidResponseBuilder
        {
            BodyWriter = writer,
            SetContentType = ct => contentType = ct,
            SetStatusCode = sc => statusCode = sc
        };

        await LiquidResponseMiddleware.HandleRequestAsync(request, response);
        await writer.FlushAsync();

        memoryStream.Position = 0;
        using var reader = new StreamReader(memoryStream, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();

        return new RenderedResponse(statusCode, contentType, body);
    }

    private async Task<bool> EnsureWebViewReadyForScriptAsync()
    {
#if WINDOWS
        if (Handler?.PlatformView is Microsoft.UI.Xaml.Controls.WebView2 nativeWebView)
        {
            if (nativeWebView.CoreWebView2 == null)
            {
                await nativeWebView.EnsureCoreWebView2Async();
            }

            return nativeWebView.CoreWebView2 != null;
        }
#endif

        return true;
    }

    private static string NormalizePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return "/";
        }

        return path.StartsWith('/') ? path : $"/{path}";
    }

    private static bool IsStaticAssetRequest(string path)
    {
        return Path.HasExtension(path);
    }

    private static string GetContentType(string path)
    {
        return Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".css" => "text/css",
            ".js" => "application/javascript",
            ".html" => "text/html",
            ".json" => "application/json",
            ".svg" => "image/svg+xml",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            _ => "application/octet-stream"
        };
    }

    private readonly record struct RenderedResponse(int StatusCode, string ContentType, string Body);
}

public sealed class LiquidPagesResponseEventArgs : EventArgs
{
    public LiquidPagesResponseEventArgs(string path, int statusCode, string contentType)
    {
        Path = path;
        StatusCode = statusCode;
        ContentType = contentType;
    }

    public string Path { get; }

    public int StatusCode { get; }

    public string ContentType { get; }
}
