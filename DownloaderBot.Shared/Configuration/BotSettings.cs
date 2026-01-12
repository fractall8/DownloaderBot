namespace DownloaderBot.Shared.Configuration;

public class BotSettings
{
    // default settings
    public string[] AllowedDomains { get; set; } = [];

    public int MaxConcurrentDownloads { get; set; } = 3;

    public int MaxFileSizeMb { get; set; } = 50;

    public int MaxVideoDurationMins { get; set; } = 120;

    public int CacheTtlDays { get; set; } = 30;

    public int MaxUserQueueSize { get; set; } = 5;

    public int LimitMessageIntervalSecs { get; set; } = 10;

    public BotCommands Commands { get; set; } = new BotCommands();
}