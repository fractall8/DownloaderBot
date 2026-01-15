using DownloaderBot.Shared.Services;

namespace DownloaderBot.Worker.Pipeline.Steps;

public class CleanupStep(IBotResponseService responseService, ILogger<CleanupStep> logger) : IProcessingStep
{
    public Task ExecuteAsync(ProcessingContext processingContext)
    {
        var filePath = processingContext.DownloadFilePath;

        if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
        {
            try
            {
                File.Delete(filePath);
                logger.LogInformation("Temp file deleted: {Path}", filePath);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to delete temp file");
            }
        }

        return Task.CompletedTask;
    }
}