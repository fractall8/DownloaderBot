using DownloaderBot.Shared.Configuration;

using Microsoft.Extensions.Options;

using StackExchange.Redis;

namespace DownloaderBot.Shared.Services;

public class UserQueueService(
    IConnectionMultiplexer redis,
    IOptions<BotSettings> settigs) : IUserQueueService
{
    private readonly int maxQueueSize = settigs.Value.MaxUserQueueSize;

    public async Task<bool> TryAddToQueueAsync(long chatId)
    {
        var db = redis.GetDatabase();
        string key = $"user_queue_count:{chatId}";

        long currentCount = await db.StringIncrementAsync(key);

        if (currentCount == 1)
        {
            await db.KeyExpireAsync(key, TimeSpan.FromHours(1));
        }

        if (currentCount > maxQueueSize)
        {
            await db.StringDecrementAsync(key);
            return false;
        }

        return true;
    }

    public async Task ReleaseSlotAsync(long chatId)
    {
        var db = redis.GetDatabase();
        string key = $"user_queue_count:{chatId}";

        long newVal = await db.StringDecrementAsync(key);
        if (newVal < 0)
        {
            await db.StringSetAsync(key, 0);
        }
    }
}