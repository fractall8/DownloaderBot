using DownloaderBot.Shared.Models;
using DownloaderBot.Worker.Models;

namespace DownloaderBot.Worker.Services;

public interface IDownloaderService
{
    Task<VideoInfo> GetVideoInfoAsync(string url);

    Task<DownloadResult> DownloadAsync(string url);
}