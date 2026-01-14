using DownloaderBot.Shared.Configuration;
using DownloaderBot.Shared.Repositories;

using Microsoft.Extensions.Options;

namespace DownloaderBot.Worker.Pipeline.Steps;

public class CacheAddStep(
    ICacheRepository cacheRepository,
    IOptions<BotSettings> settings,
    ILogger<CacheAddStep> logger) : IProcessingStep
{
    public async Task ExecuteAsync(ProcessingContext processingContext)
    {
        var sentMessage = processingContext.AudioMessage;
        var info = processingContext.VideoInfo;

        if (sentMessage is { Audio.FileId: { } newFileId } && !string.IsNullOrEmpty(info?.Id))
        {
            await cacheRepository.SetCachedFileIdAsync(
                videoId: info.Id,
                fileId: newFileId,
                ttl: TimeSpan.FromDays(settings.Value.CacheTtlDays));
            logger.LogInformation("Cached new fileId for {Id}", info.Id);
        }
    }
}