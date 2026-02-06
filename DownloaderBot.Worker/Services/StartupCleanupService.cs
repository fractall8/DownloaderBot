using DownloaderBot.Shared.Configuration;

using Microsoft.Extensions.Options;

namespace DownloaderBot.Worker.Services;

public class StartupCleanupService(IOptions<BotSettings> settings, ILogger<StartupCleanupService> logger) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Running clean up service...");
        var path = Path.Combine(Directory.GetCurrentDirectory(), settings.Value.DownloadsPath);

        if (!Directory.Exists(path))
        {
            logger.LogInformation("Directory doesn't exist. Creating...");
            Directory.CreateDirectory(path);
        }

        try
        {
            var files = Directory.GetFiles(path);
            if (files.Length == 0)
            {
                return Task.CompletedTask;
            }

            foreach (var file in files)
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Failed to delete file: {File}", file);
                }
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Cleanup service failed.");
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}