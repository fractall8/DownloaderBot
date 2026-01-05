using DownloaderBot.Worker.Services;

namespace DownloaderBot.Worker;

public class Worker(ILogger<Worker> logger, IDownloaderService downloader) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            logger.LogInformation("Test download started");
            string path = await downloader.DownloadAsync("https://www.youtube.com/watch?v=2n_tQWOb0sM");
            logger.LogInformation("Saved at path: {Path}", path);

            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Test failed");
        }
         
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }
}