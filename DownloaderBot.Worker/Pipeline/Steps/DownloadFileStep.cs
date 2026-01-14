using DownloaderBot.Shared.Services;
using DownloaderBot.Worker.Services;

namespace DownloaderBot.Worker.Pipeline.Steps;

public class DownloadFileStep(IBotResponseService responseService, IDownloaderService downloaderService) : IProcessingStep
{
    public async Task ExecuteAsync(ProcessingContext processingContext)
    {
        var task = processingContext.Task;

        await responseService.EditStatusMessageAsync(task.ChatId, task.StatusMessageId, "⬇️ Downloading your audio...");

        var result = await downloaderService.DownloadAsync(task.Url);
        processingContext.DownloadFilePath = result.FilePath;
        processingContext.FileName = $"{result.Title}.mp3";
    }
}