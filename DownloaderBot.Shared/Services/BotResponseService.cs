using DownloaderBot.Shared.Configuration;

using Microsoft.Extensions.Options;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DownloaderBot.Shared.Services;

public class BotResponseService(ITelegramBotClient botClient, IOptions<BotSettings> settings) : IBotResponseService
{
    public async Task SendPrivateWelcomeAsync(long chatId, string userName)
    {
        var text =
            $"👋 Hi, {userName}!\n\n" +
            "I'm music downloader bot. 🎧\n\n" +
            "Simply send me a link from YouTube, TikTok or SoundCloud, " +
            "and I'll respond with MP3 audio file";

        await botClient.SendMessage(chatId: chatId, text: text);
    }

    public async Task<User> GetBotInfoAsync()
    {
        return await botClient.GetMe();
    }

    public async Task SendGroupWelcomeAsync(long chatId, User[] newChatMembers)
    {
        var me = await botClient.GetMe();

        if (newChatMembers.Any(u => u.Id == me.Id))
        {
            var text =
                "👋 Hi everyone! I'm music downloader bot. 🎧\n\n" +
                "I can download audio from links to YouTube, TikTok or SoundCloud\n" +
                "To use me simply send a link to this chat " +
                "with the command \n<code>/get</code>\n\n" +
                "<i>Example:</i>\n" +
                "<code>/get https://youtu.be/...</code>";

            await botClient.SendMessage(chatId: chatId, text: text, parseMode: ParseMode.Html);
        }
    }

    public async Task<Message> SendQueuedMessageAsync(long chatId, int replyToMessageId)
    {
        return await botClient.SendMessage(
            chatId: chatId,
            text: "⏳ Link added to queue",
            replyParameters: new ReplyParameters { MessageId = replyToMessageId });
    }

    public async Task<Message> SendMessageAsync(long chatId, string text, int? replyToMessageId = null)
    {
        if (replyToMessageId is { } id)
        {
            return await botClient.SendMessage(
                chatId: chatId,
                text: text,
                replyParameters: new ReplyParameters { MessageId = id });
        }

        return await botClient.SendMessage(
            chatId: chatId,
            text: text);
    }

    public async Task EditStatusMessageAsync(long chatId, int messageId, string text)
    {
        await botClient.EditMessageText(
            chatId: chatId,
            messageId: messageId,
            text: text);
    }

    public async Task<Message> SendAudioFileAsync(long chatId, Stream fileStream, string title, int replyToMessageId)
    {
        var inputFile = InputFile.FromStream(fileStream, title);

        return await botClient.SendAudio(
            chatId: chatId,
            audio: inputFile,
            replyParameters: new ReplyParameters { MessageId = replyToMessageId });
    }

    public async Task SendCachedAudioFileAsync(long chatId, string fileId, int replyToMessageId)
    {
        var inputFile = InputFile.FromFileId(fileId);

        await botClient.SendAudio(
            chatId: chatId,
            audio: inputFile,
            replyParameters: new ReplyParameters { MessageId = replyToMessageId });
    }

    public async Task DeleteMessageAsync(long chatId, int messageId)
    {
        await botClient.DeleteMessage(
            chatId: chatId,
            messageId: messageId);
    }

    public async Task SendInvalidLinkAsync(long chatId, int messageId)
    {
        string supportedDomains = string.Join(", ", settings.Value.AllowedDomains);
        string text =
            "🧐 Seems like this link is invalid or domain isn't supported\n\n" +
            "Supported domains:\n" +
            $"<i>{supportedDomains}</i>";

        await botClient.SendMessage(
            chatId: chatId,
            text: text,
            replyParameters: new ReplyParameters { MessageId = messageId },
            parseMode: ParseMode.Html);
    }
}