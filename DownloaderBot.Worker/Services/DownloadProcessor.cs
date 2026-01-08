using DownloaderBot.Shared.Models;
using DownloaderBot.Shared.Services;

namespace DownloaderBot.Worker.Services;

public class DownloadProcessor(ILogger<DownloadProcessor> logger, IDownloaderService downloader, IBotResponseService responseService) : IDownloadProcessor
{
    public async Task ProcessAsync(DownloadTask task, CancellationToken stoppingToken)
    {
        string filePath = string.Empty;
        try
        {
            await responseService.EditStatusMessageAsync(task.ChatId, task.StatusMessageId, "Downloading your audio...");

            var result = await downloader.DownloadAsync(task.Url);
            filePath = result.FilePath;
            var prettyTitle = $"{result.Title}.mp3";

            await responseService.EditStatusMessageAsync(task.ChatId, task.StatusMessageId, "Done! Sending audio to telegram...");

            await using var fileStream = File.OpenRead(filePath);

            await responseService.SendAudioFileAsync(task.ChatId, fileStream, prettyTitle, task.ReplyToMessageId);
            await responseService.DeleteMessageAsync(task.ChatId, task.StatusMessageId);

            logger.LogInformation("File sent successfully.");
            fileStream.Close();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to process task");

            await responseService.EditStatusMessageAsync(task.ChatId, task.StatusMessageId, "Failed to download audio");
        }
        finally
        {
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}