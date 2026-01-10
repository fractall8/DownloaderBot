using System.Diagnostics.CodeAnalysis;

using DownloaderBot.Shared.Configuration;

using Microsoft.Extensions.Options;

namespace DownloaderBot.Api.Services;

public class LinkValidatorService(IOptions<BotSettings> settings) : ILinkValidatorService
{
    public bool IsValid([NotNullWhen(true)] string? link)
    {
        if (string.IsNullOrWhiteSpace(link))
        {
            return false;
        }

        if (!Uri.TryCreate(link, UriKind.Absolute, out var uri))
        {
            return false;
        }

        if (uri.Scheme != Uri.UriSchemeHttps)
        {
            return false;
        }

        return settings.Value.AllowedDomains.Any(domain =>
            uri.Host.Equals(domain, StringComparison.OrdinalIgnoreCase) ||
            uri.Host.EndsWith($".{domain}", StringComparison.OrdinalIgnoreCase));
    }
}