using DownloaderBot.Shared.Configuration;

using Microsoft.Extensions.Options;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DownloaderBot.Api.Services;

public class WebhookStartupService(
    ITelegramBotClient botClient,
    IOptions<BotSettings> settings,
    ILogger<WebhookStartupService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var config = settings.Value;
        string webHookUrl = $"{config.HostAddress}/api/bot/webhook";
        var privateCommands = new List<BotCommand>
        {
            new() { Command = "start", Description = "Start the bot", },
            new() { Command = config.Commands.HelpCommand, Description = "Show help message", },
        };

        await botClient.SetMyCommands(
            commands: privateCommands,
            scope: new BotCommandScopeAllPrivateChats(),
            cancellationToken: cancellationToken);

        var groupCommands = new List<BotCommand>
        {
            new() { Command = config.Commands.DownloadInGroupCommand, Description = "Download audio (in group)", },
            new() { Command = config.Commands.HelpCommand, Description = "Show help message", },
        };

        await botClient.SetMyCommands(
            commands: groupCommands,
            scope: new BotCommandScopeAllGroupChats(),
            cancellationToken: cancellationToken);

        await botClient.SetWebhook(
            url: webHookUrl,
            allowedUpdates: [UpdateType.Message, UpdateType.MyChatMember],
            secretToken: config.SecretToken,
            cancellationToken: cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await botClient.DeleteWebhook(cancellationToken: cancellationToken);
    }
}