using DownloaderBot.Shared.Configuration;
using DownloaderBot.Shared.Services;

using Microsoft.Extensions.Options;

namespace DownloaderBot.Worker.Pipeline.Steps;

public class FileSizeValidatorStep(IBotResponseService responseService, IOptions<BotSettings> settings) : IProcessingStep
{
    public async Task ExecuteAsync(ProcessingContext processingContext)
    {
        long maxSizeBytes = settings.Value.MaxFileSizeMb * 1024 * 1024;

        var info = processingContext.VideoInfo;
        var task = processingContext.Task;

        if (info != null && info.FileSizeBytes > maxSizeBytes)
        {
            var sizeMb = info.FileSizeBytes / 1024 / 1024;
            string errorText = $"❌ File is too big!\n" +
                               $"Size: {sizeMb} MB\n" +
                               $"Limit: {settings.Value.MaxFileSizeMb} MB";

            await responseService.EditStatusMessageAsync(
                chatId: task.ChatId,
                messageId: task.StatusMessageId,
                text: errorText);

            processingContext.ShouldStop = true;
        }
    }
}