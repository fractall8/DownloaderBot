namespace DownloaderBot.Shared.Configuration;

public class BotSettings
{
    // default settings
    public string[] AllowedDomains { get; init; } = [];

    public int MaxConcurrentDownloads { get; init; } = 3;

    public int MaxFileSizeMb { get; init; } = 50;

    public int MaxVideoDurationMins { get; init; } = 120;

    public int CacheTtlDays { get; init; } = 30;

    public int MaxUserQueueSize { get; init; } = 5;

    public int LimitMessageIntervalSecs { get; init; } = 10;

    public string SecretToken { get; init; } = string.Empty;

    public string HostAddress { get; init; } = string.Empty;

    public BotCommands Commands { get; init; } = new();
}