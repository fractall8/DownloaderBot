namespace DownloaderBot.Worker.Services;

public interface IDownloaderService
{
    Task<string> DownloadAsync(string url);
}