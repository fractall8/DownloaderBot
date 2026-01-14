using DownloaderBot.Shared.Models;
using DownloaderBot.Worker.Models;

using Telegram.Bot.Types;

namespace DownloaderBot.Worker.Pipeline;

public class ProcessingContext
{
    public required DownloadTask Task { get; set; }

    public VideoInfo? VideoInfo { get; set; }

    public Message? AudioMessage { get; set; }

    public string? DownloadFilePath { get; set; }

    public string? FileName { get; set; }

    public bool ShouldStop { get; set; }
}