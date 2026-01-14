using DownloaderBot.Shared.Configuration;
using DownloaderBot.Shared.Repositories;

using Microsoft.Extensions.Options;

namespace DownloaderBot.Shared.Services;

public class UserQueueService(
    IUserLimitRepository userLimitRepository,
    IOptions<BotSettings> settigs) : IUserQueueService
{
    private readonly int maxQueueSize = settigs.Value.MaxUserQueueSize;

    public async Task<bool> TryAddToQueueAsync(long chatId)
    {
        return await userLimitRepository.TryIncrementUserActiveTasksAsync(chatId, maxQueueSize);
    }

    public async Task ReleaseSlotAsync(long chatId)
    {
        await userLimitRepository.DecrementUserActiveTasksAsync(chatId);
    }
}