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
                if (task == null) continue;

                var filePath = await downloader.DownloadAsync(task.Url);

                await using var fileStream = File.OpenRead(filePath);
                var fileName = Path.GetFileName(filePath);

                await botClient.SendAudio(
                    chatId: task.ChatId,
                    audio: InputFile.FromStream(fileStream, fileName),
                    caption: "Downloaded track",
                    replyParameters: new ReplyParameters { MessageId = task.MessageId },
                    cancellationToken: stoppingToken
                );

                logger.LogInformation("File sent successfully.");

                fileStream.Close();
                File.Delete(filePath);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to process task");
            }
        }
    }
}