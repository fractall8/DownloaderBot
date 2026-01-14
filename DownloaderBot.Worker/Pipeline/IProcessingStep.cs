namespace DownloaderBot.Worker.Pipeline;

public interface IProcessingStep
{
    public Task ExecuteAsync(ProcessingContext processingContext);
}