namespace DownloaderBot.Shared.Repositories;

public interface ICacheRepository
{
    Task<string?> GetCachedFileIdAsync(string videoId);

    Task SetCachedFileIdAsync(string videoId, string fileId, TimeSpan ttl);
}