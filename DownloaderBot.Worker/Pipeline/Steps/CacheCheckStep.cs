using DownloaderBot.Shared.Repositories;
using DownloaderBot.Shared.Services;

namespace DownloaderBot.Worker.Pipeline.Steps;

public class CacheCheckStep(ICacheRepository cacheRepository, IBotResponseService responseService, ILogger<CacheCheckStep> logger) : IProcessingStep
{
    public async Task ExecuteAsync(ProcessingContext processingContext)
    {
        var info = processingContext.VideoInfo;
        var task = processingContext.Task;

        if (info == null)
        {
            return;
        }

        var cachedFileId = await cacheRepository.GetCachedFileIdAsync(info.Id);

        if (cachedFileId != null)
        {
            logger.LogInformation("Cache Hit! Sending via FileId: {Id}", info.Id);

            await responseService.SendCachedAudioFileAsync(
                task.ChatId,
                cachedFileId,
                task.ReplyToMessageId);

            await responseService.DeleteMessageAsync(task.ChatId, task.StatusMessageId);
            processingContext.ShouldStop = true;
        }
    }
}