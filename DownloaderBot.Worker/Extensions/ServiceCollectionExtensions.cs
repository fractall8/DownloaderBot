using DownloaderBot.Worker.Models;
using DownloaderBot.Worker.Pipeline;
using DownloaderBot.Worker.Pipeline.Steps;

using FluentValidation;

namespace DownloaderBot.Worker.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPipelineSteps(this IServiceCollection services)
    {
        services.AddScoped<GetVideoInfoStep>();
        services.AddScoped<IValidator<VideoInfo>, VideoInfoValidator>();
        services.AddScoped<ValidationStep>();
        services.AddScoped<CacheCheckStep>();
        services.AddScoped<DownloadFileStep>();
        services.AddScoped<UploadToTelegramStep>();
        services.AddScoped<CacheAddStep>();

        services.AddScoped<CleanupStep>();

        return services;
    }
}