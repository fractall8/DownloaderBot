using DownloaderBot.Api.Services;
using DownloaderBot.Shared.Configuration;
using DownloaderBot.Shared.Models;
using DownloaderBot.Shared.Repositories;
using DownloaderBot.Shared.Services;

using MediatR;

using Microsoft.Extensions.Options;

using Telegram.Bot.Types.Enums;

namespace DownloaderBot.Api.Features.ProcessTelegramUpdate;

public class ProcessTelegramUpdateHandler(
    ICommandParserService commandParserService,
    ILinkValidatorService linkValidatorService,
    IBotResponseService responseService,
    ITaskRepository taskRepository,
    IUserLimitRepository limitRepository,
    ISettingsRepository settingsRepository,
    IOptions<BotSettings> settings,
    ILogger<ProcessTelegramUpdateHandler> logger) : IRequestHandler<ProcessTelegramUpdateCommand>
{
    public async Task Handle(ProcessTelegramUpdateCommand request, CancellationToken cancellationToken)
    {
        var update = request.Update;
        if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery?.Message?.Chat.Id != null)
        {
            logger.LogInformation("Received callback query");
            await responseService.HandleSettingsCallbackAsync(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery);
        }

        if (update.Message is not { } message)
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

        if (message.Text?.StartsWith($"/{settings.Value.Commands.SettingsCommand}") == true && message.Chat.Type != ChatType.Private)
        {
            await responseService.SendSettingsMessageAsync(message.Chat.Id);
            return;
        }

        // Response to adding bot to group
        if (message.NewChatMembers is { Length: > 0 } newChatMembers)
        {
            await responseService.SendGroupWelcomeAsync(message.Chat.Id, newChatMembers);
            return;
        }

        string? downloadUrl = await commandParserService.ParseDownloadUrlAsync(message);
        if (string.IsNullOrEmpty(downloadUrl))
        {
            return;
        }

        if (!linkValidatorService.IsValid(downloadUrl))
        {
            var mode = await settingsRepository.GetChatMode(message.Chat.Id);
            if (mode == "commands")
            {
                await responseService.SendInvalidLinkAsync(message.Chat.Id, message.MessageId);
            }

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
            logger.LogInformation("Task queued successfully.\nUrl: {Url}\nUsername: {User} From chat: {ChatId}", downloadUrl, message.Chat.Username, message.Chat.Id);
        }
    }
}