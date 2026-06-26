using Kinetq.LiquidPages.Interfaces;
using Kinetq.LiquidPages.Router.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Windows.ApplicationModel.WindowsAppRuntime;

namespace Kinetq.LiquidPages.Avalonia.Sample;

public partial class MainPage : ContentPage
{
	private readonly LiquidPagesWebView _browser;

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
		InitializeComponent();

		_browser = new LiquidPagesWebView
		{
			RouteTree = routeTree,
			LiquidResponseMiddleware = liquidResponseMiddleware,
			TemplateOptionsManager = templateOptionsManager
		};
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

	private static (IRouteTree RouteTree, ILiquidResponseMiddleware LiquidResponseMiddleware, ITemplateOptionsManager TemplateOptionsManager) ResolveServices()
	{
		var serviceProvider = ServiceRegistration.CreateServiceProvider();
		return (
			serviceProvider.GetRequiredService<IRouteTree>(),
			serviceProvider.GetRequiredService<ILiquidResponseMiddleware>(),
			serviceProvider.GetRequiredService<ITemplateOptionsManager>());
	}
}
