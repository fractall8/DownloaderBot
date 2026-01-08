using Telegram.Bot.Types;

namespace DownloaderBot.Api.Services;

public interface IBotResponseService
{
    Task SendPrivateWelcomeAsync(long chatId, string userName);

    Task SendGroupWelcomeAsync(long chatId, User[] newChatMembers);

    Task<Message> SendQueuedMessageAsync(long chatId, int replyToMessageId);
}