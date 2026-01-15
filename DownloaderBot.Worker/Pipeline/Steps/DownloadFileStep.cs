using DownloaderBot.Shared.Services;
using DownloaderBot.Worker.Services;

namespace DownloaderBot.Worker.Pipeline.Steps;

public class DownloadFileStep(
    IBotResponseService responseService,
    IDownloaderService downloaderService,
    ILogger<DownloadFileStep> logger) : IProcessingStep
{
    public async Task ExecuteAsync(ProcessingContext processingContext)
    {
        var task = processingContext.Task;

        try
        {
            await responseService.EditMessageAsync(task.ChatId, task.StatusMessageId, "⬇️ Downloading your audio...");

            var result = await downloaderService.DownloadAsync(task.Url);
            processingContext.DownloadFilePath = result.FilePath;
            processingContext.FileName = $"{result.Title}.mp3";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to download video: {Message}", ex.Message);

            string errorText = "❌ Failed to download video. Please try again later.";
            await responseService.EditMessageAsync(task.ChatId, task.StatusMessageId, errorText);
            processingContext.ShouldStop = true;
        }
    }
}