using Microsoft.Extensions.Logging;

namespace MauiApp;

public static class MauiProgram
{
	/// <summary>Khởi tạo ứng dụng, font và logging cho môi trường Debug.</summary>
	// Dùng tên kiểu đầy đủ để tránh xung đột với namespace MauiApp của dự án.
	public static Microsoft.Maui.Hosting.MauiApp CreateMauiApp()
	{
		var builder = Microsoft.Maui.Hosting.MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
