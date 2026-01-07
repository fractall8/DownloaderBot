using DownloaderBot.Shared;

namespace DownloaderBot.Worker.Services;

public interface IDownloaderService
{
    Task<DownloadResult> DownloadAsync(string url);
}