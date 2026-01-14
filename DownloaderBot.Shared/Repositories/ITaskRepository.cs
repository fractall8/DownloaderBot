using DownloaderBot.Shared.Models;

namespace DownloaderBot.Shared.Repositories;

public interface ITaskRepository
{
    Task EnqueueTaskAsync(DownloadTask task);

    Task<DownloadTask?> DequeueTaskAsync();
}