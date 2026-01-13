using System.Text.Json;

using DownloaderBot.Shared.Models;

using StackExchange.Redis;

namespace DownloaderBot.Shared.Repositories;

public class RedisRepository(IConnectionMultiplexer redis)
    : ITaskRepository, ICacheRepository, IUserLimitRepository
{
    private readonly IDatabase db = redis.GetDatabase();

    public async Task EnqueueTaskAsync(DownloadTask task)
    {
        var json = JsonSerializer.Serialize(task);
        await db.ListRightPushAsync("downloads", json);
    }

    public async Task<DownloadTask?> DequeueTaskAsync()
    {
        var json = await db.ListLeftPopAsync("downloads");
        return json.IsNullOrEmpty ? null : JsonSerializer.Deserialize<DownloadTask>(json.ToString());
    }

    public async Task<string?> GetCachedFileIdAsync(string videoId)
    {
        var val = await db.StringGetAsync($"audio_cache:{videoId}");
        return val.HasValue ? val.ToString() : null;
    }

    public async Task SetCachedFileIdAsync(string videoId, string fileId, TimeSpan ttl)
    {
        await db.StringSetAsync($"audio_cache:{videoId}", fileId, ttl);
    }

    public async Task<bool> TryIncrementUserActiveTasksAsync(long chatId, int maxLimit)
    {
        string key = $"user_queue_count:{chatId}";
        long current = await db.StringIncrementAsync(key);

        if (current == 1)
        {
            await db.KeyExpireAsync(key, TimeSpan.FromHours(1));
        }

        if (current > maxLimit)
        {
            await db.StringDecrementAsync(key);
            return false;
        }

        return true;
    }

    public async Task DecrementUserActiveTasksAsync(long chatId)
    {
        string key = $"user_queue_count:{chatId}";
        var val = await db.StringDecrementAsync(key);
        if (val < 0)
        {
            await db.StringSetAsync(key, 0);
        }
    }

    public async Task<bool> IsWarningOnCooldownAsync(long chatId)
    {
        return await db.KeyExistsAsync($"warn_limit:{chatId}");
    }

    public async Task SetWarningCooldownAsync(long chatId, TimeSpan ttl)
    {
        await db.StringSetAsync($"warn_limit:{chatId}", "1", ttl);
    }
}