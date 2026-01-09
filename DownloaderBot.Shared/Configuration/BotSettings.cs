namespace DownloaderBot.Shared.Configuration;

public class BotSettings
{
    // default settings
    public string[] AllowedDomains { get; set; } = [];

    public int MaxConcurrentDownloads { get; set; } = 3;

    public int MaxFileSizeMb { get; set; } = 50;
}