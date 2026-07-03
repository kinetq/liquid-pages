using System.Text;
using Kinetq.LiquidPages.Interfaces;
using Kinetq.LiquidPages.Maui.Interfaces;
using Kinetq.LiquidPages.Models;

namespace Kinetq.LiquidPages.Maui;

public class LiquidPagesWebView : WebView
{
    private readonly IRouteTree _routeTree;
    private readonly ILiquidResponseMiddleware _liquidResponseMiddleware;

    public event EventHandler<LiquidPagesResponseEventArgs>? ResponseRendered;

    public LiquidPagesWebView(IRouteTree routeTree, ILiquidResponseMiddleware liquidResponseMiddleware)
    {
        _routeTree = routeTree;
        _liquidResponseMiddleware = liquidResponseMiddleware;

        Loaded += OnLoaded;
        Navigating += OnNavigating;
    }

    public Task NavigateToPathAsync(string? path)
    {
        return OpenPathAsync(path);
    }

    private async void OnLoaded(object? sender, EventArgs e)
    {
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
            Html = response.Body,
        };
        
        OnResponseRendered(normalizedPath, response);
    }

    private void OnResponseRendered(string path, RenderedResponse response)
    {
        ResponseRendered?.Invoke(this, new LiquidPagesResponseEventArgs(path, response.StatusCode, response.ContentType));
    }

    private async Task<RenderedResponse> HandlePathAsync(string path)
    {
        var routeMatch = _routeTree.Match(path);
        var request = new LiquidRequestModel
        {
            Route = path,
            Method = "GET",
            LiquidRoute = routeMatch?.LiquidRoute,
            RouteValues = routeMatch?.RouteValues ?? EmptyRouteValuesDictionary.Instance
        };

        await using var memoryStream = new MemoryStream();
        await using var writer = new StreamWriter(memoryStream, Encoding.UTF8, leaveOpen: true);

        var mauiResponse = new MauiLiquidResponse();
        var response = new MauiLiquidResponseBuilder(mauiResponse, writer);

        await _liquidResponseMiddleware.HandleRequestAsync(request, response);
        await writer.FlushAsync();

        memoryStream.Position = 0;
        using var reader = new StreamReader(memoryStream, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();

        return new RenderedResponse(mauiResponse.StatusCode, mauiResponse.ContentType, body);
    }

    private static string NormalizePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return "/";
        }

        return path.StartsWith('/') ? path : $"/{path}";
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
