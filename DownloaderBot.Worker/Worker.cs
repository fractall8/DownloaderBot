using System.Text.Json;

using DownloaderBot.Shared;
using DownloaderBot.Worker.Services;

using StackExchange.Redis;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace DownloaderBot.Worker;

public class Worker(ILogger<Worker> logger, IDownloaderService downloader, ITelegramBotClient botClient, IConnectionMultiplexer redis) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var db = redis.GetDatabase();
        logger.LogInformation("Worker started. Waiting for tasks...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var json = await db.ListLeftPopAsync("downloads");

                if (json.IsNullOrEmpty)
                {
                    await Task.Delay(1000, stoppingToken);
                    continue;
                }

                var task = JsonSerializer.Deserialize<DownloadTask>(json.ToString());
                if (task == null)
                {
                    continue;
                }

                string filePath = string.Empty;
                try
                {
                    await botClient.EditMessageText(
                        chatId: task.ChatId,
                        messageId: task.StatusMessageId,
                        text: "Downloading your audio...",
                        cancellationToken: stoppingToken);

                    var result = await downloader.DownloadAsync(task.Url);
                    filePath = result.FilePath;
                    var prettyTitle = $"{result.Title}.mp3";

                    await botClient.EditMessageText(
                        chatId: task.ChatId,
                        messageId: task.StatusMessageId,
                        text: "Done! Sending audio to telegram...",
                        cancellationToken: stoppingToken);

                    await using var fileStream = File.OpenRead(filePath);
                    var inputFile = InputFile.FromStream(fileStream, prettyTitle);

                    await botClient.SendAudio(
                        chatId: task.ChatId,
                        audio: inputFile,
                        replyParameters: new ReplyParameters { MessageId = task.ReplyToMessageId },
                        cancellationToken: stoppingToken);

                    await botClient.DeleteMessage(
                        chatId: task.ChatId,
                        messageId: task.StatusMessageId,
                        cancellationToken: stoppingToken);

                    logger.LogInformation("File sent successfully.");
                    fileStream.Close();
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Failed to process task");

                    await botClient.EditMessageText(
                        chatId: task.ChatId,
                        messageId: task.StatusMessageId,
                        text: "Failed to download audio",
                        cancellationToken: stoppingToken);
                }
                finally
                {
                    if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Critical worker error");
            }
        }
    }
}