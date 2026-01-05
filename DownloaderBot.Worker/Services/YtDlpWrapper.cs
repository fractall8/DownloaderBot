using System.Diagnostics;

namespace DownloaderBot.Worker.Services;

public class YtDlpWrapper : IDownloaderService
{
    private readonly ILogger<YtDlpWrapper> _logger;
    
    public YtDlpWrapper(ILogger<YtDlpWrapper> logger)
    {
        _logger = logger;
    }

    public async Task<string> DownloadAsync(string url)
    {
        var fileName = $"{Guid.NewGuid()}.mp3";
        var outputTemplate = $"/app/downloads/{fileName}";

        var args = $"-x --audio-format mp3 -o \"{outputTemplate}\" \"{url}\"";
        
        var startInfo = new ProcessStartInfo
        {
            FileName = "yt-dlp",
            Arguments = args,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        
        _logger.LogInformation("Starting download: {Url}", url);

        using (var process = new Process())
        {
            process.StartInfo = startInfo;
            process.Start();
        
            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
        
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                _logger.LogError("yt-dlp error: {Error}", error);
                throw new Exception($"Download failed. Exit code: {process.ExitCode}");
            }
        };

        _logger.LogInformation("Download completed: {FileName}", fileName);
        
        return outputTemplate;
    }
}