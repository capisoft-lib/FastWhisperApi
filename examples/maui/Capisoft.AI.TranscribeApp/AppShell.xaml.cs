using Capisoft.AI.TranscribeApp.Pages;

namespace Capisoft.AI.TranscribeApp;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute(nameof(TranscribePage), typeof(TranscribePage));
		Routing.RegisterRoute(nameof(TranslatePage), typeof(TranslatePage));
	}
}
