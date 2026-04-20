namespace DownloaderBot.Shared.Helpers;

public static class FileHelpers
{
    public static string SanitizeFileName(string? originalName)
    {
        if (string.IsNullOrWhiteSpace(originalName))
        {
            return "unknown_audio";
        }

        var invalidChars = Path.GetInvalidFileNameChars();
        var safeName = new string(originalName.Where(c => !invalidChars.Contains(c)).ToArray());

        safeName = safeName.Replace("\"", "'");
        safeName = safeName.TrimEnd(' ', '.');

        if (safeName.Length > 200)
        {
            safeName = safeName.Substring(0, 200);
        }

        return string.IsNullOrEmpty(safeName) ? "unknown_audio" : safeName;
    }
}