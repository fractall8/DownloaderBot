using MediatR;

using Telegram.Bot.Types;

namespace DownloaderBot.Api.Features.ProcessTelegramUpdate;

public record ProcessTelegramUpdateCommand(Update Update) : IRequest;