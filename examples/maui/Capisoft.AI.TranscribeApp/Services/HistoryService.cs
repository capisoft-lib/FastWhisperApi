using Capisoft.AI.TranscribeApp.Models;
using SQLite;

namespace Capisoft.AI.TranscribeApp.Services;

public class HistoryService
{
    private readonly string _dbPath = Path.Combine(FileSystem.AppDataDirectory, "history.db3");
    private SQLiteAsyncConnection? _connection;

    private async Task<SQLiteAsyncConnection> GetConnectionAsync()
    {
        if (_connection is not null)
        {
            return _connection;
        }

        Directory.CreateDirectory(FileSystem.AppDataDirectory);
        _connection = new SQLiteAsyncConnection(_dbPath);
        await _connection.CreateTableAsync<ConversationItem>();
        return _connection;
    }

    public async Task<IReadOnlyList<ConversationItem>> GetByModeAsync(string mode)
    {
        var connection = await GetConnectionAsync();

        var records = await connection.Table<ConversationItem>()
            .Where(item => item.Mode == mode)
            .OrderByDescending(item => item.CreatedAtUtc)
            .ToListAsync();

        return records;
    }

    public async Task AddAsync(ConversationItem item)
    {
        var connection = await GetConnectionAsync();
        await connection.InsertAsync(item);
    }

    public async Task ClearModeAsync(string mode)
    {
        var connection = await GetConnectionAsync();

        var items = await connection.Table<ConversationItem>()
            .Where(item => item.Mode == mode)
            .ToListAsync();

        foreach (var item in items)
        {
            if (File.Exists(item.AudioPath))
            {
                File.Delete(item.AudioPath);
            }
        }

        await connection.ExecuteAsync("DELETE FROM ConversationItems WHERE Mode = ?", mode);
    }
}
