using DownloaderBot.Shared.Configuration;
using DownloaderBot.Shared.Services;

using Microsoft.Extensions.Options;

namespace DownloaderBot.Worker.Pipeline.Steps;

public class VideoDurationValidatorStep(IBotResponseService responseService, IOptions<BotSettings> settings) : IProcessingStep
{
    public async Task ExecuteAsync(ProcessingContext processingContext)
    {
        int maxVideoDurationSeconds = settings.Value.MaxVideoDurationMins * 60;

        var info = processingContext.VideoInfo;
        var task = processingContext.Task;

        if (info is { FileSizeBytes: null } && info.DurationSeconds > maxVideoDurationSeconds)
        {
            await responseService.EditStatusMessageAsync(task.ChatId, task.StatusMessageId, "❌ Video is too long.");

            processingContext.ShouldStop = true;
        }
    }
}