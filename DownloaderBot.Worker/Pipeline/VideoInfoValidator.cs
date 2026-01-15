using DownloaderBot.Shared.Configuration;
using DownloaderBot.Worker.Models;

using FluentValidation;

using Microsoft.Extensions.Options;

namespace DownloaderBot.Worker.Pipeline;

public class VideoInfoValidator : AbstractValidator<VideoInfo>
{
    public VideoInfoValidator(IOptions<BotSettings> options)
    {
        var settings = options.Value;

        RuleFor(x => x.IsLive)
            .NotEqual(true)
            .WithMessage("❌ This is a live stream. I can download audio only from finished videos");

        RuleFor(x => x.FileSizeBytes)
            .LessThanOrEqualTo(settings.MaxFileSizeMb * 1024 * 1024)
            .WithMessage($"❌ File is too big! Limit: {settings.MaxFileSizeMb} MB");

        RuleFor(x => x.DurationSeconds)
            .LessThanOrEqualTo(settings.MaxVideoDurationMins * 60)
            .WithMessage("❌ Video is too long.");
    }
}