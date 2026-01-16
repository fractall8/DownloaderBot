using DownloaderBot.Shared.Configuration;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DownloaderBot.Shared.Services;

public class BotResponseService(
    ITelegramBotClient botClient,
    IOptions<BotSettings> settings,
    ILogger<BotResponseService> logger) : IBotResponseService
{
    public async Task SendPrivateWelcomeAsync(Message message)
    {
        string name = message.From?.Username ?? message.From?.FirstName ?? "Friend";
        var text =
            $"👋 Hi, {name}!\n\n" +
            "I'm music downloader bot. 🎧\n\n" +
            "Simply send me a link from YouTube, TikTok or SoundCloud, " +
            "and I'll respond with MP3 audio file\n\n" +
            $"<i>If you need more information use /{settings.Value.Commands.HelpCommand}</i>";

        await SendMessageAsync(chatId: message.Chat.Id, text: text, replyToMessageId: message.Id);
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
                "with the command \n<code>/get</code>\n" +
                "<i>Example:</i>\n" +
                "<code>/get https://youtu.be/...</code>\n\n" +
                $"<i>If you need more information use /{settings.Value.Commands.HelpCommand}</i>";

            await SendMessageAsync(chatId: chatId, text: text);
        }
    }

    public async Task SendHelpMessageAsync(long chatId)
    {
        string supportedDomains = string.Join(", ", settings.Value.AllowedDomains);
        var text =
            "<b>📖 Help & FAQ</b>\n\n" +
            "I can download audio from:\n" +
            $"<i>{supportedDomains}</i>\n\n" +
            "<b>Limits:</b>\n" +
            $"Max duration: {settings.Value.MaxVideoDurationMins} mins\n" +
            $"Max file size: {settings.Value.MaxFileSizeMb} MB\n" +
            $"Max downloads in queue from user: {settings.Value.MaxUserQueueSize}\n\n" +
            "<b>Usage:</b>\n" +
            "In private messages: <i>just send a link to bot</i>\n" +
            $"In groups/chats: /{settings.Value.Commands.DownloadInGroupCommand} [link]";

        await SendMessageAsync(chatId: chatId, text: text);
    }

    public async Task<Message?> SendQueuedMessageAsync(long chatId, int replyToMessageId)
    {
        return await SendMessageAsync(
            chatId: chatId,
            text: "⏳ Link added to queue",
            replyToMessageId: replyToMessageId);
    }

    public async Task<Message?> SendAudioFileAsync(long chatId, Stream fileStream, string title, int replyToMessageId)
    {
        try
        {
            var inputFile = InputFile.FromStream(fileStream, title);

            return await botClient.SendAudio(
                chatId: chatId,
                audio: inputFile,
                replyParameters: new ReplyParameters
                {
                    MessageId = replyToMessageId,
                    AllowSendingWithoutReply = true,
                });
        }
        catch (ApiRequestException ex)
        {
            logger.LogError(ex, "Error while sending audio file: {Message}", ex.Message);
            return null;
        }
    }

    public async Task SendCachedAudioFileAsync(long chatId, string fileId, int replyToMessageId)
    {
        try
        {
            var inputFile = InputFile.FromFileId(fileId);
            await botClient.SendAudio(
                chatId: chatId,
                audio: inputFile,
                replyParameters: new ReplyParameters
                {
                    MessageId = replyToMessageId,
                    AllowSendingWithoutReply = true,
                });
        }
        catch (ApiRequestException ex)
        {
            logger.LogError(ex, "Error sending cached audio: {Msg}", ex.Message);
        }
    }

    public async Task DeleteMessageAsync(long chatId, int messageId)
    {
        try
        {
            await botClient.DeleteMessage(
                chatId: chatId,
                messageId: messageId);
        }
        catch (ApiRequestException)
        {
        }
    }

    public async Task SendInvalidLinkAsync(long chatId, int messageId)
    {
        string supportedDomains = string.Join(", ", settings.Value.AllowedDomains);
        string text =
            "🧐 Seems like this link is invalid or domain isn't supported\n\n" +
            "Supported domains:\n" +
            $"<i>{supportedDomains}</i>";

        await SendMessageAsync(
            chatId: chatId,
            text: text,
            replyToMessageId: messageId);
    }

    public async Task<Message?> SendMessageAsync(long chatId, string text, int? replyToMessageId = null)
    {
        try
        {
            var replyParams = replyToMessageId.HasValue
                ? new ReplyParameters { MessageId = replyToMessageId.Value, AllowSendingWithoutReply = true }
                : null;

            return await botClient.SendMessage(
                chatId: chatId,
                text: text,
                replyParameters: replyParams,
                parseMode: ParseMode.Html);
        }
        catch (ApiRequestException ex)
        {
            logger.LogError(ex, "Error while sending message: {Message}", ex.Message);
        }

        return null;
    }

    public async Task EditMessageAsync(long chatId, int messageId, string text)
    {
        try
        {
            await botClient.EditMessageText(chatId, messageId, text);
        }
        catch (ApiRequestException ex)
        {
            logger.LogError(ex, "Error while editing message: {Message}", ex.Message);
        }
    }
}