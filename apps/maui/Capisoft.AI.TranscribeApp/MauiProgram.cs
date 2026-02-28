using Microsoft.Extensions.Logging;
using Capisoft.AI.TranscribeApp.Services;
using Plugin.Maui.Audio;

namespace Capisoft.AI.TranscribeApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.AddAudio()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		builder.Services.AddSingleton<SettingsService>();
		builder.Services.AddSingleton<HistoryService>();
		builder.Services.AddSingleton(new HttpClient());
		builder.Services.AddSingleton<WhisperApiService>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		var app = builder.Build();
		ServiceHelper.Configure(app.Services);
		return app;
	}
}
