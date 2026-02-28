using System.Net.Http.Headers;
using System.Text.Json;
using Capisoft.AI.TranscribeApp.Models;

namespace Capisoft.AI.TranscribeApp.Services;

public class WhisperApiService
{
    private readonly HttpClient _httpClient;
    private readonly SettingsService _settingsService;

    public WhisperApiService(HttpClient httpClient, SettingsService settingsService)
    {
        _httpClient = httpClient;
        _settingsService = settingsService;
    }

    public async Task<string> SendAudioAsync(string endpoint, string audioPath, CancellationToken cancellationToken = default)
    {
        var settings = await _settingsService.GetAsync();
        var baseUrl = settings.ServerUrl.Trim().TrimEnd('/');

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new InvalidOperationException("Server URL is empty.");
        }

        var requestUri = $"{baseUrl}/{endpoint}";

        await using var fileStream = File.OpenRead(audioPath);
        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(fileStream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");
        content.Add(streamContent, "file", Path.GetFileName(audioPath));

        using var response = await _httpClient.PostAsync(requestUri, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        using var jsonDocument = JsonDocument.Parse(payload);

        if (!jsonDocument.RootElement.TryGetProperty("text", out var textElement))
        {
            throw new InvalidOperationException("API response does not contain 'text'.");
        }

        return textElement.GetString() ?? string.Empty;
    }
}
