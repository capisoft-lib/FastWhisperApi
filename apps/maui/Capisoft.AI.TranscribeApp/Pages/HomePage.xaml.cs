using Capisoft.AI.TranscribeApp.Models;
using Capisoft.AI.TranscribeApp.Services;

namespace Capisoft.AI.TranscribeApp.Pages;

public partial class HomePage : ContentPage
{
    private readonly SettingsService _settingsService;

    public HomePage()
    {
        InitializeComponent();
        _settingsService = ServiceHelper.GetRequiredService<SettingsService>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var settings = await _settingsService.GetAsync();
        ServerEntry.Text = settings.ServerUrl;
    }

    private async void OnSaveServerClicked(object sender, EventArgs e)
    {
        var value = ServerEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(value))
        {
            await DisplayAlertAsync("Invalid server", "Please enter a server URL.", "OK");
            return;
        }

        await _settingsService.SaveAsync(new AppSettings { ServerUrl = value });
        await DisplayAlertAsync("Saved", "Server URL updated.", "OK");
    }

    private async void OnTranscribeClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(TranscribePage));
    }

    private async void OnTranslateClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(TranslatePage));
    }
}
