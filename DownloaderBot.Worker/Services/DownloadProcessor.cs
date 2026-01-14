using DownloaderBot.Shared.Models;
using DownloaderBot.Shared.Repositories;
using DownloaderBot.Worker.Pipeline;
using DownloaderBot.Worker.Pipeline.Steps;

namespace DownloaderBot.Worker.Services;

public class DownloadProcessor(
    IServiceProvider services,
    IUserLimitRepository limitRepository,
    ILogger<DownloadProcessor> logger) : IDownloadProcessor
{
    public async Task ProcessAsync(DownloadTask task, CancellationToken stoppingToken)
    {
        var context = new ProcessingContext { Task = task };

        var steps = new List<Type>
        {
            typeof(GetVideoInfoStep),
            typeof(LiveStreamValidator),
            typeof(FileSizeValidatorStep),
            typeof(VideoDurationValidatorStep),
            typeof(CacheCheckStep),
            typeof(DownloadFileStep),
            typeof(UploadToTelegramStep),
            typeof(CacheAddStep),
            typeof(CleanupStep),
        };

        try
        {
            using var scope = services.CreateScope();

            foreach (var stepType in steps)
            {
                if (context.ShouldStop)
                {
                    break;
                }

                var step = (IProcessingStep)scope.ServiceProvider.GetRequiredService(stepType);
                await step.ExecuteAsync(context);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Download pipeline failed");
        }
        finally
        {
            using var scope = services.CreateScope();
            var cleanup = scope.ServiceProvider.GetRequiredService<CleanupStep>();
            await cleanup.ExecuteAsync(context);

            await limitRepository.DecrementUserActiveTasksAsync(task.ChatId);
        }
    }
}