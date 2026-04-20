using DownloaderBot.Shared.Helpers;
using DownloaderBot.Shared.Services;

namespace DownloaderBot.Worker.Pipeline.Steps;

public class UploadToTelegramStep(IBotResponseService responseService) : IProcessingStep
{
    public async Task ExecuteAsync(ProcessingContext processingContext)
    {
        var task = processingContext.Task;

        await responseService.EditMessageAsync(task.ChatId, task.StatusMessageId, "⬆️ Done! Sending audio to telegram...");

        if (processingContext.DownloadFilePath != null)
        {
            await using var fileStream = File.OpenRead(processingContext.DownloadFilePath);

            var safeFileName = FileHelpers.SanitizeFileName(processingContext.FileName);

            processingContext.AudioMessage = await responseService.SendAudioFileAsync(task.ChatId, fileStream, safeFileName, task.ReplyToMessageId);
        }

        await responseService.DeleteMessageAsync(task.ChatId, task.StatusMessageId);
    }
}