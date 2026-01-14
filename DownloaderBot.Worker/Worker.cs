using DownloaderBot.Shared.Configuration;
using DownloaderBot.Shared.Models;
using DownloaderBot.Shared.Repositories;
using DownloaderBot.Worker.Services;

using Microsoft.Extensions.Options;

namespace DownloaderBot.Worker;

public class Worker(
    ILogger<Worker> logger,
    IDownloadProcessor processor,
    ITaskRepository taskRepository,
    IOptions<BotSettings> settings) : BackgroundService
{
    private readonly SemaphoreSlim semaphore = new(settings.Value.MaxConcurrentDownloads);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Worker started. Concurrent for {Tasks} tasks. Waiting for tasks...", settings.Value.MaxConcurrentDownloads);

        while (!stoppingToken.IsCancellationRequested)
        {
            await semaphore.WaitAsync(stoppingToken);

            try
            {
                var task = await taskRepository.DequeueTaskAsync();

                if (task == null)
                {
                    semaphore.Release();
                    await Task.Delay(1000, stoppingToken);
                    continue;
                }

                _ = RunTaskWrapperAsync(task, stoppingToken);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error in worker main loop");
                semaphore.Release();
            }
        }
    }

    private async Task RunTaskWrapperAsync(DownloadTask task, CancellationToken token)
    {
        try
        {
            await processor.ProcessAsync(task, token);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled error in task processor");
        }
        finally
        {
            semaphore.Release();
        }
    }
}