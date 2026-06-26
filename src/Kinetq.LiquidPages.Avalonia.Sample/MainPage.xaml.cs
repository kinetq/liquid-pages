using Kinetq.LiquidPages.Builders;
using Kinetq.LiquidPages.Interfaces;
using Kinetq.LiquidPages.Models;
using Kinetq.LiquidPages.Router.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Windows.ApplicationModel.WindowsAppRuntime;
using System.Text;

namespace Kinetq.LiquidPages.Avalonia.Sample;

public partial class MainPage : ContentPage
{
	private readonly IRouteTree _routeTree;
	private readonly ILiquidResponseMiddleware _liquidResponseMiddleware;
	private readonly ITemplateOptionsManager _templateOptionsManager;
	private string _currentRoute = "/";
	private bool _webViewReady;

	public MainPage()
		: this(ResolveServices())
	{
	}

	private MainPage((IRouteTree RouteTree, ILiquidResponseMiddleware LiquidResponseMiddleware, ITemplateOptionsManager TemplateOptionsManager) services)
		: this(services.RouteTree, services.LiquidResponseMiddleware, services.TemplateOptionsManager)
	{
	}

	public MainPage(
		IRouteTree routeTree,
		ILiquidResponseMiddleware liquidResponseMiddleware,
		ITemplateOptionsManager templateOptionsManager)
	{
		_routeTree = routeTree;
		_liquidResponseMiddleware = liquidResponseMiddleware;
		_templateOptionsManager = templateOptionsManager;

		InitializeComponent();

		AddressEntry.Text = "/";

		Browser.Loaded += async (_, _) =>
		{
			_webViewReady = true; 
            var response = await HandlePathAsync("/");
            Browser.Source = new HtmlWebViewSource
            {
                Html = response.Body
            };
        };

        Browser.Navigating += async (_, args) =>
        {
            Uri uri = new Uri(args.Url);
            if (uri.Scheme == "data" || uri.Scheme == "about")
            {
                return;
            }

            args.Cancel = true;

            var response = await HandlePathAsync(uri.AbsolutePath);
            Browser.Source = new HtmlWebViewSource
            {
                Html = response.Body
            };
        };
    }

	private async void NavigateButtonOnClicked(object? sender, EventArgs e)
	{
		await OpenPathAsync(AddressEntry.Text);
	}

	private async Task OpenPathAsync(string? path)
	{
		var normalizedPath = NormalizePath(path);
		var response = await HandlePathAsync(normalizedPath);
		await RenderInBrowserAsync(response);

		HtmlPreviewEditor.Text = $"Status: {response.StatusCode}{Environment.NewLine}Content-Type: {response.ContentType}";
		AddressEntry.Text = normalizedPath;
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

		var templateOptions = sourceRouteMatch.LiquidRoute.TemplateOptions
			?? _templateOptionsManager.GetTemplateOptions(sourceRouteMatch.LiquidRoute.RouteTemplate);
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

	private async Task RenderInBrowserAsync(RenderedResponse response)
	{
		if (!_webViewReady)
		{
			return;
		}

		var html = response.ContentType.StartsWith("text/html", StringComparison.OrdinalIgnoreCase)
			? response.Body
			: $"<html><body><pre>{System.Net.WebUtility.HtmlEncode(response.Body)}</pre></body></html>";

		var htmlBytes = Encoding.UTF8.GetBytes(html);
		var statusBytes = Encoding.UTF8.GetBytes($"Status: {response.StatusCode} | Content-Type: {response.ContentType}");

		var htmlBase64 = Convert.ToBase64String(htmlBytes);
		var statusBase64 = Convert.ToBase64String(statusBytes);

		if (!await EnsureWebViewReadyForScriptAsync())
		{
			Browser.Source = new HtmlWebViewSource
			{
				Html = html
			};
			return;
		}

		try
		{
			await Browser.EvaluateJavaScriptAsync($"window.renderContentFromBase64('{htmlBase64}','{statusBase64}')");
		}
		catch (InvalidOperationException)
		{
			Browser.Source = new HtmlWebViewSource
			{
				Html = html
			};
		}
	}

	private async Task<bool> EnsureWebViewReadyForScriptAsync()
	{
#if WINDOWS
		if (Browser.Handler?.PlatformView is Microsoft.UI.Xaml.Controls.WebView2 nativeWebView)
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

	private static (IRouteTree RouteTree, ILiquidResponseMiddleware LiquidResponseMiddleware, ITemplateOptionsManager TemplateOptionsManager) ResolveServices()
	{
		var serviceProvider = ServiceRegistration.CreateServiceProvider();
		return (
			serviceProvider.GetRequiredService<IRouteTree>(),
			serviceProvider.GetRequiredService<ILiquidResponseMiddleware>(),
			serviceProvider.GetRequiredService<ITemplateOptionsManager>());
	}
}
