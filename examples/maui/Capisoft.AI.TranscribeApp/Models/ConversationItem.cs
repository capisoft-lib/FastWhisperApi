using SQLite;

namespace Capisoft.AI.TranscribeApp.Models;

[Table("ConversationItems")]
public class ConversationItem
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull]
    public string Mode { get; set; } = string.Empty;

    [NotNull]
    public string AudioPath { get; set; } = string.Empty;

    [NotNull]
    public string Transcription { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    [Ignore]
    public DateTime CreatedAtLocal => CreatedAtUtc.ToLocalTime();
}
