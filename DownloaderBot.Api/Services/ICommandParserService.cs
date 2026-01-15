using Telegram.Bot.Types;

namespace DownloaderBot.Api.Services;

public interface ICommandParserService
{
    Task<string?> ParseDownloadUrlAsync(Message message);
}