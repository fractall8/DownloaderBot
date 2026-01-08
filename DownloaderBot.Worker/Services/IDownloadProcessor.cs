using DownloaderBot.Shared.Models;

namespace DownloaderBot.Worker.Services;

public interface IDownloadProcessor
{
    Task ProcessAsync(DownloadTask task, CancellationToken stoppingToken);
}