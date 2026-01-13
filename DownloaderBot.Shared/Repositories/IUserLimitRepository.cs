namespace DownloaderBot.Shared.Repositories;

public interface IUserLimitRepository
{
    Task<bool> TryIncrementUserActiveTasksAsync(long chatId, int maxLimit);

    Task DecrementUserActiveTasksAsync(long chatId);

    Task<bool> IsWarningOnCooldownAsync(long chatId);

    Task SetWarningCooldownAsync(long chatId, TimeSpan ttl);
}