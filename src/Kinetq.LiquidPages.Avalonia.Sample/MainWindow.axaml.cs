using Avalonia.Controls;
using Avalonia.Interactivity;
using Kinetq.LiquidPages.Builders;
using Kinetq.LiquidPages.Interfaces;
using Kinetq.LiquidPages.Models;
using Kinetq.LiquidPages.Router.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using Kinetq.LiquidPages.Router.Helpers;

namespace Kinetq.LiquidPages.Avalonia.Sample;

public partial class MainWindow : Window
{
    private readonly IRouteTree _routeTree;
    private readonly ILiquidResponseMiddleware _liquidResponseMiddleware;
    private readonly ITemplateOptionsManager _templateOptionsManager;
    private readonly NativeWebView? _browser;
    private string _currentRoute = "/";

    public MainWindow()
        : this(
            ResolveServices().RouteTree,
            ResolveServices().LiquidResponseMiddleware,
            ResolveServices().TemplateOptionsManager)
    {
    }

    public MainWindow(
        IRouteTree routeTree,
        ILiquidResponseMiddleware liquidResponseMiddleware,
        ITemplateOptionsManager templateOptionsManager)
    {
        _routeTree = routeTree;
        _liquidResponseMiddleware = liquidResponseMiddleware;
        _templateOptionsManager = templateOptionsManager;
        InitializeComponent();
        _browser = this.FindControl<NativeWebView>("Browser");
        
        _browser.NavigationStarted += async (sender, args) =>
        {
            if (args.Request == null || args.Request.Scheme == "data" || args.Request.Scheme == "about")
            {
                return;
            }
            
            args.Cancel = true;
            
            var path = args.Request.OriginalString.RemoveAppScheme();
            await OpenPathAsync(path);
        };

        _browser.WebResourceRequested += async (sender, args) =>
        {
            var path = args.Request.Uri;
            var response = await TryResolveStaticAssetAsync(path.ToString().RemoveAppScheme());
        };

        var navigateButton = this.FindControl<Button>("NavigateButton")
                             ?? throw new InvalidOperationException("NavigateButton not found.");
        navigateButton.Click += NavigateButtonOnClick;
    }

    private async void NavigateButtonOnClick(object? sender, RoutedEventArgs e)
    {
        var addressTextBox = this.FindControl<TextBox>("AddressTextBox");
        var path = addressTextBox?.Text;

        await OpenPathAsync(path);
    }

    private async Task OpenPathAsync(string? path)
    {
        var normalizedPath = NormalizePath(path);
        var response = await HandlePathAsync(normalizedPath);
        await RenderInBrowserAsync(response);

        var previewTextBox = this.FindControl<TextBox>("HtmlPreviewTextBox")
            ?? throw new InvalidOperationException("HtmlPreviewTextBox not found.");

        previewTextBox.Text = $"Status: {response.StatusCode}\nContent-Type: {response.ContentType}";

        //var addressTextBox = this.FindControl<TextBox>("AddressTextBox");
        //if (addressTextBox != null)
        //{
        //    addressTextBox.Text = normalizedPath;
        //}
    }

    private async Task<RenderedResponse> HandlePathAsync(string path)
    {
        if (IsStaticAssetRequest(path))
        {
            var staticResponse = await TryResolveStaticAssetAsync(path);
            if (staticResponse != null)
            {
                return staticResponse.Value;
            }
        }

        var routeMatch = _routeTree.Match(path);
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

        await _liquidResponseMiddleware.HandleRequestAsync(request, response);
        await writer.FlushAsync();

        memoryStream.Position = 0;
        using var reader = new StreamReader(memoryStream, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();

        if (routeMatch?.LiquidRoute?.RouteTemplate is { Length: > 0 } routeTemplate)
        {
            _currentRoute = routeTemplate;
        }

        return new RenderedResponse(statusCode, contentType, body);
    }

    private async Task<RenderedResponse?> TryResolveStaticAssetAsync(string path)
    {
        var sourceRouteMatch = _routeTree.Match(_currentRoute) ?? _routeTree.Match("/");
        if (sourceRouteMatch?.LiquidRoute?.RouteTemplate == null)
        {
            return null;
        }

        var templateOptions = sourceRouteMatch?.LiquidRoute?.TemplateOptions ?? _templateOptionsManager.GetTemplateOptions(sourceRouteMatch.LiquidRoute.RouteTemplate);
        if (templateOptions?.FileProvider == null)
        {
            return null;
        }

        var relativePath = path.TrimStart('/');
        var fileInfo = templateOptions.FileProvider.GetFileInfo(relativePath);
        if (!fileInfo.Exists)
        {
            return null;
        }

        await using var stream = fileInfo.CreateReadStream();
        await using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);

        var contentType = GetContentType(path);
        if (contentType.StartsWith("text/", StringComparison.OrdinalIgnoreCase))
        {
            var text = Encoding.UTF8.GetString(memoryStream.ToArray());
            return new RenderedResponse(200, contentType, text);
        }

        return new RenderedResponse(200, contentType, $"Binary response ({memoryStream.Length} bytes)");
    }

    private Task RenderInBrowserAsync(RenderedResponse response)
    {
        if (_browser == null)
        {
            return Task.CompletedTask;
        }

        var html = response.ContentType.StartsWith("text/html", StringComparison.OrdinalIgnoreCase)
            ? response.Body
            : $"<html><body><pre>{System.Net.WebUtility.HtmlEncode(response.Body)}</pre></body></html>";

        _browser.NavigateToString(html);
        return Task.CompletedTask;
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

    private static (IRouteTree RouteTree, ILiquidResponseMiddleware LiquidResponseMiddleware, ITemplateOptionsManager TemplateOptionsManager) ResolveServices()
    {
        var serviceProvider = ServiceRegistration.CreateServiceProvider();
        return (
            serviceProvider.GetRequiredService<IRouteTree>(),
            serviceProvider.GetRequiredService<ILiquidResponseMiddleware>(),
            serviceProvider.GetRequiredService<ITemplateOptionsManager>());
    }
}
