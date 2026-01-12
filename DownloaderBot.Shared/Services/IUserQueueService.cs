namespace DownloaderBot.Shared.Services;

public interface IUserQueueService
{
    Task<bool> TryAddToQueueAsync(long chatId);

    Task ReleaseSlotAsync(long chatId);
}