using Telegram.Bot.Types;

namespace DownloaderBot.Shared.Services;

public interface IBotResponseService
{
    Task<User> GetBotInfoAsync();

    Task SendPrivateWelcomeAsync(long chatId, string userName);

    Task SendGroupWelcomeAsync(long chatId, User[] newChatMembers);

    Task<Message> SendQueuedMessageAsync(long chatId, int replyToMessageId);

    Task EditStatusMessageAsync(long chatId, int messageId, string text);

    Task SendAudioFileAsync(long chatId, Stream fileStream, string title, int replyToMessageId);

    Task DeleteMessageAsync(long chatId, int messageId);

    Task SendInvalidLinkAsync(long chatId, int messageId);
}