using StackExchange.Redis;

namespace DownloaderBot.Shared.Repositories;

public class SettingsRepository(IConnectionMultiplexer redis) : ISettingsRepository
{
    private readonly IDatabase database = redis.GetDatabase();

    public async Task<string> GetChatMode(long chatId)
    {
        var key = $"chat:{chatId}:mode";
        var mode = await database.StringGetAsync(key);

        return mode.HasValue ? mode.ToString() : "commands";
    }

    public async Task UpdateChatMode(long chatId, string mode)
    {
        var key = $"chat:{chatId}:mode";
        await database.StringSetAsync(key, mode);
    }
}