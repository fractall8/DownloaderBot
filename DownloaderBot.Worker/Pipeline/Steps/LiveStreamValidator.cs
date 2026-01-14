using DownloaderBot.Shared.Services;

namespace DownloaderBot.Worker.Pipeline.Steps;

public class LiveStreamValidator(IBotResponseService responseService) : IProcessingStep
{
    public async Task ExecuteAsync(ProcessingContext processingContext)
    {
        var info = processingContext.VideoInfo;
        var task = processingContext.Task;
        if (info is { IsLive: true })
        {
            await responseService.EditStatusMessageAsync(
                task.ChatId,
                task.StatusMessageId,
                "❌ This is a live stream. I can download audio only from finished videos");
            processingContext.ShouldStop = true;
        }
    }
}