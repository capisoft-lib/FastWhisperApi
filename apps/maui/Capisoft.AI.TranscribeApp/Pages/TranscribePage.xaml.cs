using System.Collections.ObjectModel;
using Capisoft.AI.TranscribeApp.Models;
using Capisoft.AI.TranscribeApp.Services;
using Plugin.Maui.Audio;

namespace Capisoft.AI.TranscribeApp.Pages;

public partial class TranscribePage : ContentPage
{
    private readonly WhisperApiService _apiService;
    private readonly HistoryService _historyService;
    private readonly IAudioManager _audioManager;
    private readonly IAudioRecorder _audioRecorder;
    private readonly ObservableCollection<ConversationItem> _history = [];
    private IAudioPlayer? _currentPlayer;
    private MemoryStream? _playbackStream;
    private bool _isRecording;
    private bool _isBusy;

    public TranscribePage()
    {
        InitializeComponent();

        _apiService = ServiceHelper.GetRequiredService<WhisperApiService>();
        _historyService = ServiceHelper.GetRequiredService<HistoryService>();
        _audioManager = ServiceHelper.GetRequiredService<IAudioManager>();
        _audioRecorder = _audioManager.CreateRecorder();

        HistoryCollectionView.ItemsSource = _history;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadHistoryAsync();
    }

    private async Task LoadHistoryAsync()
    {
        var items = await _historyService.GetByModeAsync("transcribe");
        _history.Clear();

        foreach (var item in items)
        {
            _history.Add(item);
        }
    }

    private async void OnRecordPressed(object sender, EventArgs e)
    {
        if (_isBusy || _isRecording)
        {
            return;
        }

        try
        {
            var hasPermission = await EnsureMicrophonePermissionAsync();
            if (!hasPermission)
            {
                return;
            }

            await _audioRecorder.StartAsync();
            _isRecording = true;
            RecordButton.Text = "Release to Stop";
        }
        catch (Exception ex)
        {
            _isRecording = false;
            RecordButton.Text = "Hold to Record";
            if (IsPermissionException(ex))
            {
                await ShowMicrophonePermissionDialogAsync();
            }
            else
            {
                await DisplayAlertAsync("Recording error", ex.Message, "OK");
            }
        }
    }

    private async Task<bool> EnsureMicrophonePermissionAsync()
    {
#if WINDOWS
        // In unpackaged Windows runs, runtime manifest lookup can fail for Permissions API.
        return true;
#else
        var current = await Permissions.CheckStatusAsync<Permissions.Microphone>();
        if (current == PermissionStatus.Granted)
        {
            return true;
        }

        var requested = await Permissions.RequestAsync<Permissions.Microphone>();
        if (requested == PermissionStatus.Granted)
        {
            return true;
        }

        await ShowMicrophonePermissionDialogAsync();
        return false;
#endif
    }

    private async Task ShowMicrophonePermissionDialogAsync()
    {
        var openSettings = await DisplayAlertAsync(
            "Microphone permission",
            "Microphone access is disabled. Enable it in app settings to record.",
            "Open Settings",
            "Cancel");

        if (!openSettings)
        {
            return;
        }

        try
        {
            AppInfo.Current.ShowSettingsUI();
        }
        catch
        {
            await DisplayAlertAsync("Settings unavailable", "Open your system settings manually and enable microphone access for this app.", "OK");
        }
    }

    private static bool IsPermissionException(Exception ex)
    {
        if (ex is UnauthorizedAccessException)
        {
            return true;
        }

        var message = ex.Message.ToLowerInvariant();
        return message.Contains("permission") || message.Contains("access denied") || message.Contains("microphone");
    }

    private async void OnRecordReleased(object sender, EventArgs e)
    {
        if (_isBusy || !_isRecording)
        {
            return;
        }

        try
        {
            SetBusy(true);
            var audioSource = await _audioRecorder.StopAsync();
            _isRecording = false;
            RecordButton.Text = "Hold to Record";

            if (audioSource is null)
            {
                await DisplayAlertAsync("Recording error", "No audio was captured.", "OK");
                return;
            }

            var path = await SaveRecordingAsync(audioSource, "transcribe");
            var text = await _apiService.SendAudioAsync("transcribe", path);

            var item = new ConversationItem
            {
                Mode = "transcribe",
                AudioPath = path,
                Transcription = text,
                CreatedAtUtc = DateTime.UtcNow
            };

            await _historyService.AddAsync(item);
            _history.Insert(0, item);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", ex.Message, "OK");
        }
        finally
        {
            SetBusy(false);
            _isRecording = false;
            RecordButton.Text = "Hold to Record";
        }
    }

    private static string BuildRecordingPath(string mode)
    {
        var recordingsDirectory = Path.Combine(FileSystem.AppDataDirectory, "recordings");
        Directory.CreateDirectory(recordingsDirectory);
        return Path.Combine(recordingsDirectory, $"{mode}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.wav");
    }

    private static async Task<string> SaveRecordingAsync(IAudioSource audioSource, string mode)
    {
        var filePath = BuildRecordingPath(mode);
        await using var input = audioSource.GetAudioStream();
        await using var output = File.Create(filePath);

        if (input is null)
        {
            throw new InvalidOperationException("Recording stream is empty.");
        }

        await input.CopyToAsync(output);
        return filePath;
    }

    private async void OnClearClicked(object sender, EventArgs e)
    {
        var shouldClear = await DisplayAlertAsync("Clear history", "Delete all entries on this page?", "Yes", "No");
        if (!shouldClear)
        {
            return;
        }

        await _historyService.ClearModeAsync("transcribe");
        _history.Clear();
    }

    private async void OnPlayClicked(object sender, EventArgs e)
    {
        if (sender is not Button button || button.CommandParameter is not string audioPath || !File.Exists(audioPath))
        {
            await DisplayAlertAsync("Audio not found", "The recorded file could not be found.", "OK");
            return;
        }

        try
        {
            _currentPlayer?.Stop();
            _currentPlayer?.Dispose();
            _playbackStream?.Dispose();

            var bytes = await File.ReadAllBytesAsync(audioPath);
            _playbackStream = new MemoryStream(bytes);
            _currentPlayer = _audioManager.CreatePlayer(_playbackStream);
            _currentPlayer.Play();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Playback error", ex.Message, "OK");
        }
    }

    private async void OnCopyClicked(object sender, EventArgs e)
    {
        if (sender is not Button button || button.CommandParameter is not string text || string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        await Clipboard.Default.SetTextAsync(text);
        await DisplayAlertAsync("Copied", "Text copied to clipboard.", "OK");
    }

    private void SetBusy(bool busy)
    {
        _isBusy = busy;
        BusyIndicator.IsVisible = busy;
        BusyIndicator.IsRunning = busy;
        RecordButton.IsEnabled = !busy;
    }
}
