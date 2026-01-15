using DownloaderBot.Shared.Configuration;
using DownloaderBot.Shared.Services;

using Microsoft.Extensions.Options;

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DownloaderBot.Api.Services;

public class CommandParserService(IBotResponseService responseService, IOptions<BotSettings> settings) : ICommandParserService
{
    public async Task<string?> ParseDownloadUrlAsync(Message message)
    {
        if (string.IsNullOrWhiteSpace(message.Text))
        {
            return null;
        }

        string downloadInGroupCommand = settings.Value.Commands.DownloadInGroupCommand;
        string commandWithSlash = $"/{downloadInGroupCommand}";

        if (message.Chat.Type == ChatType.Private)
        {
            var text = message.Text.Trim();
            if (text.StartsWith(commandWithSlash, StringComparison.OrdinalIgnoreCase))
            {
                return await ExtractUrl(message.Text, commandWithSlash);
            }

            return text;
        }

        if (message.Text.StartsWith(commandWithSlash, StringComparison.OrdinalIgnoreCase))
        {
            return await ExtractUrl(message.Text, commandWithSlash);
        }

        return null;
    }

    private async Task<string?> ExtractUrl(string messageText, string command)
    {
        var parts = messageText.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 2)
        {
            return null;
        }

        var inputCommand = parts[0];
        var url = parts[1].Trim();
        var bot = await responseService.GetBotInfoAsync();

        if (bot.Username != null)
        {
            inputCommand = inputCommand.Replace($"@{bot.Username}", string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        if (inputCommand.Equals(command, StringComparison.OrdinalIgnoreCase))
        {
            return url;
        }

        return null;
    }
}