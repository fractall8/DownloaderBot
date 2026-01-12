using System.Text.Json;

using DownloaderBot.Api.Services;
using DownloaderBot.Shared.Configuration;
using DownloaderBot.Shared.Models;
using DownloaderBot.Shared.Services;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using StackExchange.Redis;

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DownloaderBot.Api.Controllers;

[ApiController]
[Route("api/bot")]
public class TelegramController(
    IConnectionMultiplexer redis,
    IBotResponseService responseService,
    ILinkValidatorService linkValidatorService,
    IUserQueueService queueService,
    IOptions<BotSettings> settings,
    ILogger<TelegramController> logger)
    : ControllerBase
{
    [HttpPost("webhook")]
    public async Task<IActionResult> Post([FromBody] JsonElement jsonElement) // receiving Update here breaks OpenApi
    {
        var update = jsonElement.Deserialize<Update>(new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });
        if (update?.Message is not { } message)
        {
            return Ok();
        }

        // Start command response
        if (message.Text?.StartsWith("/start") == true)
        {
            string name = message.From?.Username ?? message.From?.FirstName ?? "Friend";
            await responseService.SendPrivateWelcomeAsync(message.Chat.Id, name);
            return Ok();
        }

        // Response to adding bot to group
        if (message.NewChatMembers is { Length: > 0 } newChatMembers)
        {
            await responseService.SendGroupWelcomeAsync(message.Chat.Id, newChatMembers);
            return Ok();
        }

        string? downloadUrl = null;
        string downloadInGroupCommand = settings.Value.Commands.DownloadInGroupCommand;

        if (message.Chat.Type == ChatType.Private)
        {
            string text = message.Text?.Trim() ?? string.Empty;

            if (text.StartsWith(downloadInGroupCommand, StringComparison.OrdinalIgnoreCase))
            {
                downloadUrl = await ParseUrlFromCommand(message.Text, downloadInGroupCommand);
            }
            else
            {
                downloadUrl = text;
            }
        }
        else if (message.Text?.StartsWith(downloadInGroupCommand, StringComparison.OrdinalIgnoreCase) == true)
        {
            downloadUrl = await ParseUrlFromCommand(message.Text, downloadInGroupCommand);

            if (downloadUrl == null)
            {
                return Ok();
            }
        }

        if (!linkValidatorService.IsValid(downloadUrl))
        {
            await responseService.SendInvalidLinkAsync(message.Chat.Id, message.MessageId);
            return Ok();
        }

        var db = redis.GetDatabase();

        // Check queue limit
        if (!await queueService.TryAddToQueueAsync(message.Chat.Id))
        {
            string warnKey = $"warn_limit:{message.Chat.Id}";

            if (!await db.KeyExistsAsync(warnKey))
            {
                string warnText = $"✋ Queue limit reached!\n" +
                                   $"You can have maximum {settings.Value.MaxUserQueueSize} active downloads.\n" +
                                   "Please wait for your previous downloads finish.";
                await responseService.SendMessageAsync(message.Chat.Id, warnText, message.MessageId);

                await db.StringSetAsync(warnKey, "1", TimeSpan.FromSeconds(settings.Value.LimitMessageIntervalSecs));
            }

            return Ok();
        }

        var statusMessage = await responseService.SendQueuedMessageAsync(message.Chat.Id, message.MessageId);

        var task = new DownloadTask
        {
            ChatId = message.Chat.Id,
            StatusMessageId = statusMessage.MessageId,
            ReplyToMessageId = message.MessageId,
            Url = downloadUrl,
        };

        var json = JsonSerializer.Serialize(task);

        await db.ListRightPushAsync("downloads", json);
        logger.LogInformation("Task added to Redis {Url}", downloadUrl);
        return Ok();
    }

    private async Task<string?> ParseUrlFromCommand(string? messageText, string command)
    {
        if (string.IsNullOrWhiteSpace(messageText) || string.IsNullOrWhiteSpace(command))
        {
            return null;
        }

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