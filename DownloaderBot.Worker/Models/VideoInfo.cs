namespace DownloaderBot.Worker.Models;

public class VideoInfo
{
    public string Title { get; set; } = string.Empty;

    public long? FileSizeBytes { get; set; }

    public double DurationSeconds { get; set; }

    public bool? IsLive { get; set; }
}