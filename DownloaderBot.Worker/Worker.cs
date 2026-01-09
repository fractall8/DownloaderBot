using System.Text.Json;

using DownloaderBot.Shared.Configuration;
using DownloaderBot.Shared.Models;

using DownloaderBot.Worker.Services;

using Microsoft.Extensions.Options;

using StackExchange.Redis;

namespace DownloaderBot.Worker;

public class Worker(ILogger<Worker> logger, IDownloadProcessor processor, IConnectionMultiplexer redis, IOptions<BotSettings> settings) : BackgroundService
{
    private readonly SemaphoreSlim semaphore = new(settings.Value.MaxConcurrentDownloads);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var db = redis.GetDatabase();
        logger.LogInformation("Worker started. Concurrent for {Tasks} tasks. Waiting for tasks...", settings.Value.MaxConcurrentDownloads);

        while (!stoppingToken.IsCancellationRequested)
        {
            await semaphore.WaitAsync(stoppingToken);

            try
            {
                var json = await db.ListLeftPopAsync("downloads");

                if (json.IsNullOrEmpty)
                {
                    semaphore.Release();
                    await Task.Delay(1000, stoppingToken);
                    continue;
                }

                var task = JsonSerializer.Deserialize<DownloadTask>(json.ToString());
                if (task == null)
                {
                    semaphore.Release();
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