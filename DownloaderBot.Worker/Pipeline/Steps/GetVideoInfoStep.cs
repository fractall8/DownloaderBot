using DownloaderBot.Shared.Services;
using DownloaderBot.Worker.Services;

namespace DownloaderBot.Worker.Pipeline.Steps;

public class GetVideoInfoStep(IDownloaderService downloaderService, IBotResponseService responseService) : IProcessingStep
{
    public async Task ExecuteAsync(ProcessingContext processingContext)
    {
        var task = processingContext.Task;
        await responseService.EditStatusMessageAsync(task.ChatId, task.StatusMessageId, "🔎 Checking file info...");

        var info = await downloaderService.GetVideoInfoAsync(task.Url);
        processingContext.VideoInfo = info;
    }
}