using Telegram.Bot.Types;

namespace DownloaderBot.Shared.Services;

public interface IBotResponseService
{
    Task<User> GetBotInfoAsync();

    Task SendPrivateWelcomeAsync(Message message);

    Task SendGroupWelcomeAsync(long chatId, User[] newChatMembers);

    Task<Message> SendQueuedMessageAsync(long chatId, int replyToMessageId);

    Task<Message> SendMessageAsync(long chatId, string text, int? replyToMessageId = null);

    Task EditStatusMessageAsync(long chatId, int messageId, string text);

    Task<Message> SendAudioFileAsync(long chatId, Stream fileStream, string title, int replyToMessageId);

    Task SendCachedAudioFileAsync(long chatId, string fileId, int replyToMessageId);

    Task DeleteMessageAsync(long chatId, int messageId);

    Task SendInvalidLinkAsync(long chatId, int messageId);
}