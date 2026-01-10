using System.Text.Json;

using DownloaderBot.Api.Services;
using DownloaderBot.Shared.Models;
using DownloaderBot.Shared.Services;

using Microsoft.AspNetCore.Mvc;

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

        if (message.Chat.Type == ChatType.Private)
        {
            downloadUrl = message.Text;
        }
        else if (message.Text?.StartsWith("/get ") == true) // for now hardcoded command
        {
            downloadUrl = message.Text.Replace("/get", string.Empty).Trim();
        }

        if (!linkValidatorService.IsValid(downloadUrl))
        {
            await responseService.SendInvalidLinkAsync(message.Chat.Id, message.MessageId);
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

        var db = redis.GetDatabase();
        var json = JsonSerializer.Serialize(task);

        await db.ListRightPushAsync("downloads", json);
        logger.LogInformation("Task added to Redis {Url}", downloadUrl);
        return Ok();
    }
}