using DownloaderBot.Shared.Models;
using DownloaderBot.Worker.Models;

using YoutubeDLSharp;
using YoutubeDLSharp.Options;

namespace DownloaderBot.Worker.Services;

public class YtDlpDownloaderService(ILogger<YtDlpDownloaderService> logger) : IDownloaderService
{
    private readonly YoutubeDL youtubeDL = new()
    {
        YoutubeDLPath = "yt-dlp",
        FFmpegPath = "usr/bin/ffmpeg",
        OutputFolder = "app/downloads",
    };

    public async Task<VideoInfo> GetVideoInfoAsync(string url)
    {
        var res = await youtubeDL.RunVideoDataFetch(url);
        if (!res.Success)
        {
            throw new Exception("Failed to fetch info");
        }

        var data = res.Data;
        logger.LogInformation("Resolved url: {Url}, ID: {Id}", data.Url, data.ID);

        long? size = null;

        if (data.Formats != null && data.Formats.Length > 0)
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

    public async Task<DownloadResult> DownloadAsync(string url)
    {
        var fileName = $"{Guid.NewGuid()}.mp3";
        var outputPath = Path.Combine("app/downloads", fileName);

        var videoDataResult = await youtubeDL.RunVideoDataFetch(url);

        if (!videoDataResult.Success)
        {
            throw new Exception($"Failed to fetch metadata: {string.Join('\n', videoDataResult.ErrorOutput)}");
        }

        var title = videoDataResult.Data.Title ?? "Unknown Track";
        var cleanTitle = string.Join("_", title.Split(Path.GetInvalidFileNameChars()));

        var options = new OptionSet
        {
            AudioFormat = AudioConversionFormat.Mp3,
            ExtractAudio = true,
            Output = outputPath,
            EmbedThumbnail = true,
            EmbedMetadata = true,
            ForceIPv6 = true,
        };

        logger.LogInformation("Start downloading: {Url}", url);
        var result = await youtubeDL.RunAudioDownload(url, AudioConversionFormat.Mp3, ct: CancellationToken.None, overrideOptions: options);

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
}