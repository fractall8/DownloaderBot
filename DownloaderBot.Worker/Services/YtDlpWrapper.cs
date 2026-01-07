using System.Diagnostics;
using System.Text;
using DownloaderBot.Shared;
using YoutubeDLSharp;
using YoutubeDLSharp.Options;

namespace DownloaderBot.Worker.Services;

public class YtDlpWrapper : IDownloaderService
{
    private readonly ILogger<YtDlpWrapper> _logger;
    private readonly YoutubeDL _youtubeDL;

    public YtDlpWrapper(ILogger<YtDlpWrapper> logger)
    {
        _logger = logger;
        _youtubeDL = new YoutubeDL
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

        var videoDataResult = await _youtubeDL.RunVideoDataFetch(url);

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
        
        _logger.LogInformation("Start downloading: {Url}", url);
        var result = await _youtubeDL.RunAudioDownload(url, AudioConversionFormat.Mp3, ct: CancellationToken.None, overrideOptions: options);
        
        if (!result.Success)
        {
            var errors = string.Join("\n", result.ErrorOutput);
            _logger.LogError("YoutubeDL failed: {Errors}", errors);
            throw new Exception($"Download failed: {errors}");
        }
        
        var finalPath = result.Data;
        
        if (string.IsNullOrEmpty(finalPath) || !File.Exists(finalPath))
        {
            if (File.Exists(outputPath + ".mp3")) finalPath = outputPath + ".mp3";
            else if (File.Exists(outputPath)) finalPath = outputPath;
        }
        
        _logger.LogInformation("Download completed: {Path}", finalPath);
        return new DownloadResult(finalPath, cleanTitle);
    }
}