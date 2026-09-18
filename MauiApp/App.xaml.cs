using Microsoft.Extensions.DependencyInjection;

namespace MauiApp;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	// Tạo cửa sổ chứa Shell; không lưu token hoặc trạng thái Dropbox ở tầng Application.
	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());
	}
}
