using Kinetq.LiquidPages.Interfaces;
using Kinetq.LiquidPages.Maui;
using Kinetq.LiquidPages.Maui.Interfaces;

namespace Kinetq.LiquidPages.Avalonia.Sample;

public partial class MainPage : ContentPage
{
	private readonly LiquidPagesWebView _browser;

	public MainPage()
		: this(ResolveServices())
	{
	}

	private MainPage((IRouteTree RouteTree, ILiquidResponseMiddleware LiquidResponseMiddleware) services)
		: this(services.RouteTree, services.LiquidResponseMiddleware)
	{
	}

	public MainPage(
		IRouteTree routeTree,
		ILiquidResponseMiddleware liquidResponseMiddleware)
	{
		InitializeComponent();

        _browser = new LiquidPagesWebView(routeTree, liquidResponseMiddleware);
		_browser.ResponseRendered += OnBrowserResponseRendered;
		BrowserHost.Content = _browser;

		AddressEntry.Text = "/";
	}

	private async void NavigateButtonOnClicked(object? sender, EventArgs e)
	{
		await _browser.NavigateToPathAsync(AddressEntry.Text);
	}

	private void OnBrowserResponseRendered(object? sender, LiquidPagesResponseEventArgs e)
	{
		HtmlPreviewEditor.Text = $"Status: {e.StatusCode}{Environment.NewLine}Content-Type: {e.ContentType}";
		AddressEntry.Text = e.Path;
	}

	private static (IRouteTree RouteTree, ILiquidResponseMiddleware LiquidResponseMiddleware) ResolveServices()
	{
		var serviceProvider = ServiceRegistration.CreateServiceProvider();
		return (
			serviceProvider.GetRequiredService<IRouteTree>(),
			serviceProvider.GetRequiredService<ILiquidResponseMiddleware>());
	}
}
