using DownloaderBot.Shared.Models;
using DownloaderBot.Worker.Models;

using YoutubeDLSharp;
using YoutubeDLSharp.Options;

namespace DownloaderBot.Worker.Services;

public class YtDlpDownloaderService(ILogger<YtDlpDownloaderService> logger) : IDownloaderService
{
    private readonly string cookiesPath = "/app/cookies.txt";
    private readonly string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";

    private readonly YoutubeDL youtubeDL = new()
    {
        YoutubeDLPath = "/usr/local/bin/yt-dlp",
        FFmpegPath = "/usr/bin/ffmpeg",
        OutputFolder = "/app/downloads",
    };

    [Obsolete]
    public async Task<VideoInfo> GetVideoInfoAsync(string url)
    {
        var options = GetOptions();

        var res = await youtubeDL.RunVideoDataFetch(url, overrideOptions: options);
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
            size = data.Formats
                .Where(f => f.FileSize > 0)
                .OrderByDescending(f => f.FileSize)
                .Select(f => f.FileSize)
                .FirstOrDefault();
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

    [Obsolete]
    public async Task<DownloadResult> DownloadAsync(string url)
    {
        var fileName = $"{Guid.NewGuid()}.mp3";
        var outputPath = Path.Combine("app/downloads", fileName);

        var options = GetOptions();
        var videoDataResult = await youtubeDL.RunVideoDataFetch(url, overrideOptions: options);

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

        logger.LogInformation("Start downloading: {Url}", url);
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

    [Obsolete]
    private OptionSet GetOptions()
    {
        var options = new OptionSet
        {
            NoCheckCertificates = true,
        };

        options.AddCustomOption("--add-header", $"User-Agent:{userAgent}");
        options.AddCustomOption("--remote-components", "ejs:github");

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
}