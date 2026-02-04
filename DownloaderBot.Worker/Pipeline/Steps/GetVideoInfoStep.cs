using DownloaderBot.Shared.Services;
using DownloaderBot.Worker.Models;
using DownloaderBot.Worker.Services;

namespace DownloaderBot.Worker.Pipeline.Steps;

public class GetVideoInfoStep(
    IDownloaderService downloaderService,
    IBotResponseService responseService,
    ILogger<GetVideoInfoStep> logger) : IProcessingStep
{
    public async Task ExecuteAsync(ProcessingContext processingContext)
    {
        var task = processingContext.Task;

        try
        {
            await responseService.EditMessageAsync(task.ChatId, task.StatusMessageId, "🔎 Checking file info...");

            var info = await downloaderService.GetVideoInfoAsync(task.Url);
            logger.LogInformation("File size bytes: {Bytes}", info.FileSizeBytes);
            processingContext.VideoInfo = info;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get video info: {Message}", ex.Message);

            string errorText = "❌ Failed to get video info. Try again later.";
            await responseService.EditMessageAsync(task.ChatId, task.StatusMessageId, errorText);
            processingContext.ShouldStop = true;
        }
    }
}