namespace DownloaderBot.Shared.Repositories;

public interface ISettingsRepository
{
    Task<string> GetChatMode(long chatId);

    Task UpdateChatMode(long chatId, string mode);
}