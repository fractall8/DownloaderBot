using System.Text.Json;
using DownloaderBot.Shared;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DownloaderBot.Api.Controllers;

[ApiController]
[Route("api/bot")]
public class TelegramController : ControllerBase
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<TelegramController> _logger;
    
    public TelegramController(IConnectionMultiplexer redis, ILogger<TelegramController> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> Post([FromBody] JsonElement jsonElement) // receiving Update here breaks OpenApi
    {
        var update = jsonElement.Deserialize<Update>();
        
        if (update?.Message is not { } message || message.Text is not { } text)
        {
            return Ok();
        }
        
        _logger.LogInformation("Received message from {ChatId}: {Text}", message.Chat.Id, text);

        string? downloadUrl = null;

        if (message.Chat.Type == ChatType.Private)
        {
            downloadUrl = text;
        } else if (text.StartsWith("/get ")) // for now hardcoded command
        {
            downloadUrl = text.Replace("/get ", "").Trim();
        }

        // validate url before pushing task to Redis
        // temporary no url validation 
        if (!string.IsNullOrWhiteSpace(downloadUrl))
        {
            var task = new DownloadTask
            {
                ChatId = message.Chat.Id,
                MessageId = message.MessageId,
                Url = downloadUrl
            };
            
            var db = _redis.GetDatabase();
            var json = JsonSerializer.Serialize(task);

            await db.ListRightPushAsync("downloads", json);
            
            _logger.LogInformation("Task added to Redis {Url}", downloadUrl);
        }

        return Ok();
    }
}