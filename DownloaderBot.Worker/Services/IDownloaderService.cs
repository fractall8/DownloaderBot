using DownloaderBot.Shared.Models;

namespace DownloaderBot.Worker.Services;

public interface IDownloaderService
{
    Task<DownloadResult> DownloadAsync(string url);
}