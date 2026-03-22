using DownloaderBot.Shared.Configuration;
using DownloaderBot.Shared.Models;
using DownloaderBot.Worker.Models;

using Microsoft.Extensions.Options;
using YoutubeDLSharp;
using YoutubeDLSharp.Options;

namespace DownloaderBot.Worker.Services;

public class YtDlpDownloaderService(IOptions<BotSettings> settings, ILogger<YtDlpDownloaderService> logger) : IDownloaderService
{
    private readonly string cookiesPath = "/app/cookies.txt";
    private readonly string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";

    private static HttpClient httpClient = new HttpClient();

    private readonly YoutubeDL youtubeDL = new()
    {
        YoutubeDLPath = "/usr/local/bin/yt-dlp",
        FFmpegPath = "/usr/bin/ffmpeg",
        OutputFolder = Path.Combine(Directory.GetCurrentDirectory(), settings.Value.DownloadsPath),
    };

    public async Task<VideoInfo> GetVideoInfoAsync(string url)
    {
        var options = GetOptions();

        var finalUrl = await ResolveUrlAsync(url);

        var res = await youtubeDL.RunVideoDataFetch(finalUrl, overrideOptions: options);
        if (!res.Success)
        {
            var errorMsg = string.Join('\n', res.ErrorOutput);

            logger.LogError("Failed to fetch info: {Msg}", errorMsg);
            throw new Exception($"Failed to fetch info: {errorMsg}");
        }

        var data = res.Data;

        long? size = null;

        if (data.Formats is { Length: > 0 })
        {
            var audioFormats = data.Formats
                .Where(f => f.FileSize > 0)
                .Where(f => f.VideoCodec == "none" || f.Extension == "none")
                .OrderByDescending(f => f.FileSize)
                .ToList();

            if (audioFormats.Count > 0)
            {
                size = audioFormats.First().FileSize;
            }
            else
            {
                size = data.Formats
                    .Where(f => f.FileSize > 0)
                    .OrderBy(f => f.FileSize)
                    .Select(f => f.FileSize)
                    .FirstOrDefault();
            }
        }

        return new VideoInfo
        {
            Id = data.ID,
            Title = data.Title,
            FileSizeBytes = size,
            DurationSeconds = data.Duration ?? 0,
            IsLive = data.IsLive,
        };
    }

    public async Task<DownloadResult> DownloadAsync(string url)
    {
        var fileName = $"{Guid.NewGuid()}.mp3";
        var downloadDir = Path.Combine(Directory.GetCurrentDirectory(), settings.Value.DownloadsPath);
        var outputPath = Path.Combine(downloadDir, fileName);

        var options = GetOptions();
        var finalUrl = await ResolveUrlAsync(url);
        var videoDataResult = await youtubeDL.RunVideoDataFetch(finalUrl, overrideOptions: options);

        if (!videoDataResult.Success)
        {
            throw new Exception($"Failed to fetch metadata. Check logs for OAuth code! Error: {string.Join('\n', videoDataResult.ErrorOutput)}");
        }

        var title = videoDataResult.Data.Title ?? "Unknown Track";
        var cleanTitle = string.Join("_", title.Split(Path.GetInvalidFileNameChars()));

        var downloadOptions = GetOptions();
        downloadOptions.AudioFormat = AudioConversionFormat.Mp3;
        downloadOptions.ExtractAudio = true;
        downloadOptions.Output = outputPath;
        downloadOptions.EmbedThumbnail = true;
        downloadOptions.EmbedMetadata = true;

        logger.LogInformation("Start downloading: {Url}", finalUrl);
        var result = await youtubeDL.RunAudioDownload(
            url,
            AudioConversionFormat.Mp3,
            ct: CancellationToken.None,
            overrideOptions: downloadOptions);

        if (!result.Success)
        {
            var errors = string.Join("\n", result.ErrorOutput);
            logger.LogError("YoutubeDL failed: {Errors}", errors);
            throw new Exception($"Download failed: {errors}");
        }

        var finalPath = result.Data;

        if (string.IsNullOrEmpty(finalPath) || !File.Exists(finalPath))
        {
            if (File.Exists(outputPath + ".mp3"))
            {
                finalPath = outputPath + ".mp3";
            }
            else if (File.Exists(outputPath))
            {
                finalPath = outputPath;
            }
        }

        logger.LogInformation("Download completed: {Path}", finalPath);
        return new DownloadResult(finalPath, cleanTitle);
    }

    private OptionSet GetOptions()
    {
        var options = new OptionSet
        {
            NoCheckCertificates = true,
        };

        options.AddCustomOption("--add-header", $"User-Agent:{userAgent}");
        options.AddCustomOption("--remote-components", "ejs:github");
        options.AddCustomOption("--impersonate", "chrome");

        if (File.Exists(cookiesPath))
        {
            options.Cookies = cookiesPath;
        }
        else
        {
            logger.LogWarning("Cookies file NOT FOUND at {Path}. Download will likely fail.", cookiesPath);
        }

        return options;
    }

    private async Task<string> ResolveUrlAsync(string shortUrl)
    {
        if (!settings.Value.ShortDomains.Any(domain => shortUrl.Contains(domain)))
        {
            return shortUrl;
        }

        try
        {
            var req = new HttpRequestMessage(HttpMethod.Head, shortUrl);
            var res = await httpClient.SendAsync(req);

            var finalUrl = res.RequestMessage?.RequestUri?.ToString();
            if (!string.IsNullOrEmpty(finalUrl) && finalUrl != shortUrl)
            {
                logger.LogInformation("Resolved URL: {Url}", finalUrl);
                return finalUrl;
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to resolve full url: {ShortUrl}", shortUrl);
        }

        return shortUrl;
    }
}