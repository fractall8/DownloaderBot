namespace DownloaderBot.Worker;

public class WorkerSettings
{
    // default settings
    public int MaxConcurrentDownloads { get; set; } = 3;

    public int MaxFileSizeMb { get; set; } = 50;
}