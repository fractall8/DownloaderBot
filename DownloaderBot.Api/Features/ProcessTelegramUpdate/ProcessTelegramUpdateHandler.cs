using DownloaderBot.Api.Services;
using DownloaderBot.Shared.Configuration;
using DownloaderBot.Shared.Models;
using DownloaderBot.Shared.Repositories;
using DownloaderBot.Shared.Services;

using MediatR;

using Microsoft.Extensions.Options;

namespace DownloaderBot.Api.Features.ProcessTelegramUpdate;

public class ProcessTelegramUpdateHandler(
    ICommandParserService commandParserService,
    ILinkValidatorService linkValidatorService,
    IBotResponseService responseService,
    ITaskRepository taskRepository,
    IUserLimitRepository limitRepository,
    IOptions<BotSettings> settings,
    ILogger<ProcessTelegramUpdateHandler> logger) : IRequestHandler<ProcessTelegramUpdateCommand>
{
    public async Task Handle(ProcessTelegramUpdateCommand request, CancellationToken cancellationToken)
    {
        var update = request.Update;
        if (update?.Message is not { } message)
        {
            return;
        }

        if (message.Text?.StartsWith("/start") == true)
        {
            await responseService.SendPrivateWelcomeAsync(message);
            return;
        }

        if (message.Text?.StartsWith($"/{settings.Value.Commands.HelpCommand}") == true)
        {
            await responseService.SendHelpMessageAsync(message.Chat.Id);
            return;
        }

        // Response to adding bot to group
        if (message.NewChatMembers is { Length: > 0 } newChatMembers)
        {
            await responseService.SendGroupWelcomeAsync(message.Chat.Id, newChatMembers);
            return;
        }

        string? downloadUrl = await commandParserService.ParseDownloadUrlAsync(message);
        if (downloadUrl == null)
        {
            return;
        }

        if (!linkValidatorService.IsValid(downloadUrl))
        {
            await responseService.SendInvalidLinkAsync(message.Chat.Id, message.MessageId);
            return;
        }

        // Check queue limit
        if (!await limitRepository.TryIncrementUserActiveTasksAsync(message.Chat.Id, settings.Value.MaxUserQueueSize))
        {
            if (!await limitRepository.IsWarningOnCooldownAsync(message.Chat.Id))
            {
                await responseService.SendMessageAsync(message.Chat.Id, "✋ Queue limit reached!", message.MessageId);
                await limitRepository.SetWarningCooldownAsync(message.Chat.Id, TimeSpan.FromSeconds(10));
            }

            return;
        }

        var statusMessage = await responseService.SendQueuedMessageAsync(message.Chat.Id, message.MessageId);

        if (statusMessage != null)
        {
            var task = new DownloadTask
            {
                ChatId = message.Chat.Id,
                StatusMessageId = statusMessage.MessageId,
                ReplyToMessageId = message.MessageId,
                Url = downloadUrl,
            };

            await taskRepository.EnqueueTaskAsync(task);
            logger.LogInformation("Task queued successfully. Url: {Url}, From chat: {ChatId}", downloadUrl, message.Chat.Id);
        }
    }
}