namespace DownloaderBot.Shared.Models;

public class DownloadTask
{
    public long ChatId { get; set; }

    public int ReplyToMessageId { get; set; }

    public int StatusMessageId { get; set; }

    public string Url { get; set; } = string.Empty;
}