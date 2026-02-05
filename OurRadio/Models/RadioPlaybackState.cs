namespace OurRadio.Models;

public sealed class RadioPlaybackState
{
    public int RadioId { get; set; }
    public int SongId { get; set; }
    public string Filename { get; set; } = string.Empty;
    public int DurationSeconds { get; set; }
    public DateTimeOffset StartedAtUtc { get; set; }
}
