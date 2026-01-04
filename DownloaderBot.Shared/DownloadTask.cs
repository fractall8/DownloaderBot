namespace DownloaderBot.Shared;

public class DownloadTask
{
    public long ChatId  { get; set; }
    
    public int MessageId { get; set; }
    
    public string Url { get; set; } = string.Empty;
}