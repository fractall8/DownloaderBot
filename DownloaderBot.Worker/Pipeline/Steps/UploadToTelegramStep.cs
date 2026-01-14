using DownloaderBot.Shared.Services;

namespace DownloaderBot.Worker.Pipeline.Steps;

public class UploadToTelegramStep(IBotResponseService responseService) : IProcessingStep
{
    public async Task ExecuteAsync(ProcessingContext processingContext)
    {
        var task = processingContext.Task;

        await responseService.EditStatusMessageAsync(task.ChatId, task.StatusMessageId, "⬆️ Done! Sending audio to telegram...");

        if (processingContext.DownloadFilePath != null)
        {
            await using var fileStream = File.OpenRead(processingContext.DownloadFilePath);

            processingContext.AudioMessage = await responseService.SendAudioFileAsync(task.ChatId, fileStream, processingContext.FileName ?? string.Empty, task.ReplyToMessageId);
        }

        await responseService.DeleteMessageAsync(task.ChatId, task.StatusMessageId);
    }
}