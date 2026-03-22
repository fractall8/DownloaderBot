using System.Text.Json;

using DownloaderBot.Api.Features.ProcessTelegramUpdate;
using DownloaderBot.Shared.Configuration;

using MediatR;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using Telegram.Bot.Types;

namespace DownloaderBot.Api.Controllers;

[ApiController]
[Route("api/bot")]
public class TelegramController(IOptions<BotSettings> settings) : ControllerBase
{
    [HttpPost("webhook")]
    public async Task<IActionResult> Post(
        [FromBody] JsonElement jsonElement, // receiving Update here breaks OpenApi
        [FromHeader(Name = "X-Telegram-Bot-Api-Secret-Token")] string? secretToken,
        [FromServices] IMediator mediator)
    {
        if (secretToken != settings.Value.SecretToken)
        {
            return Unauthorized();
        }

        var update = jsonElement.Deserialize<Update>(new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });
        if (update != null)
        {
            await mediator.Send(new ProcessTelegramUpdateCommand(update));
        }

        return Ok();
    }
}