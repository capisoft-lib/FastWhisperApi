using System.Text.Json;
using Capisoft.AI.TranscribeApp.Models;

namespace Capisoft.AI.TranscribeApp.Services;

public class SettingsService
{
    private const string SettingsFileName = "settings.json";
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private static readonly AppSettings DefaultSettings = new();

    private string SettingsPath => Path.Combine(FileSystem.AppDataDirectory, SettingsFileName);

    public async Task<AppSettings> GetAsync()
    {
        await EnsureSettingsFileExistsAsync();

        try
        {
            await using var stream = File.OpenRead(SettingsPath);
            var settings = await JsonSerializer.DeserializeAsync<AppSettings>(stream);
            return settings ?? DefaultSettings;
        }
        catch
        {
            await SaveAsync(DefaultSettings);
            return DefaultSettings;
        }
    }

    public async Task SaveAsync(AppSettings settings)
    {
        Directory.CreateDirectory(FileSystem.AppDataDirectory);

        await using var stream = File.Create(SettingsPath);
        await JsonSerializer.SerializeAsync(stream, settings, JsonOptions);
    }

    private async Task EnsureSettingsFileExistsAsync()
    {
        if (File.Exists(SettingsPath))
        {
            return;
        }

        Directory.CreateDirectory(FileSystem.AppDataDirectory);

        try
        {
            await using var packageStream = await FileSystem.OpenAppPackageFileAsync(SettingsFileName);
            await using var output = File.Create(SettingsPath);
            await packageStream.CopyToAsync(output);
        }
        catch
        {
            await SaveAsync(DefaultSettings);
        }
    }
}
