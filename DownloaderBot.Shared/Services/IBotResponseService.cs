using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DownloaderBot.Shared.Services;

public interface IBotResponseService
{
    Task<User> GetBotInfoAsync();

    Task SendPrivateWelcomeAsync(Message message);

    Task SendGroupWelcomeAsync(long chatId, User[] newChatMembers);

    Task SendHelpMessageAsync(long chatId);

    Task SendSettingsMessageAsync(long chatId);

    Task HandleSettingsCallbackAsync(long chatId, CallbackQuery callbackQuery);

    Task<Message?> SendQueuedMessageAsync(long chatId, int replyToMessageId);

    Task<Message?> SendMessageAsync(long chatId, string text, int? replyToMessageId = null, ReplyMarkup? replyMarkup = null);

    Task EditMessageAsync(long chatId, int messageId, string text);

    Task<Message?> SendAudioFileAsync(long chatId, Stream fileStream, string title, int replyToMessageId);

    Task SendCachedAudioFileAsync(long chatId, string fileId, int replyToMessageId);

    Task DeleteMessageAsync(long chatId, int messageId);

    Task SendInvalidLinkAsync(long chatId, int messageId);
}