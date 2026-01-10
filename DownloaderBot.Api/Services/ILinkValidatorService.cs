using System.Diagnostics.CodeAnalysis;

namespace DownloaderBot.Api.Services;

public interface ILinkValidatorService
{
    public bool IsValid([NotNullWhen(true)] string? link);
}