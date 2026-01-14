using DownloaderBot.Shared.Configuration;
using DownloaderBot.Shared.Models;
using DownloaderBot.Shared.Repositories;
using DownloaderBot.Shared.Services;

using Microsoft.Extensions.Options;

namespace DownloaderBot.Worker.Services;

public class DownloadProcessor(
    IDownloaderService downloader,
    IBotResponseService responseService,
    IUserQueueService queueService,
    ICacheRepository cacheRepository,
    IOptions<BotSettings> settings,
    ILogger<DownloadProcessor> logger) : IDownloadProcessor
{
    public async Task ProcessAsync(DownloadTask task, CancellationToken stoppingToken)
    {
        string filePath = string.Empty;
        try
        {
            long maxSizeBytes = settings.Value.MaxFileSizeMb * 1024 * 1024;
            await responseService.EditStatusMessageAsync(task.ChatId, task.StatusMessageId, "🔎 Checking file info...");

            var info = await downloader.GetVideoInfoAsync(task.Url);

            if (info.FileSizeBytes > maxSizeBytes)
            {
                var sizeMb = info.FileSizeBytes / 1024 / 1024;
                string errorText = $"❌ File is too big!\n" +
                              $"Size: {sizeMb} MB\n" +
                              $"Limit: {settings.Value.MaxFileSizeMb} MB";

                await responseService.EditStatusMessageAsync(
                    chatId: task.ChatId,
                    messageId: task.StatusMessageId,
                    text: errorText);
                return;
            }

            int maxVideoDurationSeconds = settings.Value.MaxVideoDurationMins * 60;
            if (info.FileSizeBytes == null && info.DurationSeconds > maxVideoDurationSeconds)
            {
                await responseService.EditStatusMessageAsync(task.ChatId, task.StatusMessageId, "❌ Video is too long.");
                return;
            }

            if (info.IsLive == true)
            {
                await responseService.EditStatusMessageAsync(
                    task.ChatId,
                    task.StatusMessageId,
                    "❌ This is a live stream. I can download audio only from finished videos");
                return;
            }

            var cachedFileId = await cacheRepository.GetCachedFileIdAsync(info.Id);

            if (cachedFileId != null)
            {
                logger.LogInformation("Cache Hit! Sending via FileId: {Id}", info.Id);

                await responseService.SendCachedAudioFileAsync(
                    task.ChatId,
                    cachedFileId,
                    task.ReplyToMessageId);

                await responseService.DeleteMessageAsync(task.ChatId, task.StatusMessageId);
                return;
            }

            await responseService.EditStatusMessageAsync(task.ChatId, task.StatusMessageId, "⬇️ Downloading your audio...");

            var result = await downloader.DownloadAsync(task.Url);
            filePath = result.FilePath;
            var prettyTitle = $"{result.Title}.mp3";

            await responseService.EditStatusMessageAsync(task.ChatId, task.StatusMessageId, "⬆️ Done! Sending audio to telegram...");

            await using var fileStream = File.OpenRead(filePath);

            var sentMessage = await responseService.SendAudioFileAsync(task.ChatId, fileStream, prettyTitle, task.ReplyToMessageId);

            if (sentMessage.Audio?.FileId is { } newFileId && !string.IsNullOrEmpty(info.Id))
            {
                await cacheRepository.SetCachedFileIdAsync(
                    videoId: info.Id,
                    fileId: newFileId,
                    ttl: TimeSpan.FromDays(settings.Value.CacheTtlDays));
                logger.LogInformation("Cached new fileId for {Id}", info.Id);
            }

            await responseService.DeleteMessageAsync(task.ChatId, task.StatusMessageId);
            logger.LogInformation("File sent successfully.");
            fileStream.Close();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to process task");
            await responseService.EditStatusMessageAsync(task.ChatId, task.StatusMessageId, "❌ Error: Failed to download audio");
        }
        finally
        {
            await queueService.ReleaseSlotAsync(task.ChatId);
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}