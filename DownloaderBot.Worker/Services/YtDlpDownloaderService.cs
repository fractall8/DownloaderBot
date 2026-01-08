using DownloaderBot.Shared.Models;

using YoutubeDLSharp;
using YoutubeDLSharp.Options;

namespace DownloaderBot.Worker.Services;

public class YtDlpDownloaderService : IDownloaderService
{
    private readonly ILogger<YtDlpDownloaderService> logger;
    private readonly YoutubeDL youtubeDL;

    public YtDlpDownloaderService(ILogger<YtDlpDownloaderService> logger)
    {
        this.logger = logger;
        youtubeDL = new YoutubeDL
        {
            YoutubeDLPath = "yt-dlp",
            FFmpegPath = "usr/bin/ffmpeg",
            OutputFolder = "app/downloads",
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