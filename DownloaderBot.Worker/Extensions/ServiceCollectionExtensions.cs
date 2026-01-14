using DownloaderBot.Worker.Pipeline.Steps;

namespace DownloaderBot.Worker.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPipelineSteps(this IServiceCollection services)
    {
        services.AddScoped<GetVideoInfoStep>();
        services.AddScoped<LiveStreamValidator>();
        services.AddScoped<FileSizeValidatorStep>();
        services.AddScoped<VideoDurationValidatorStep>();
        services.AddScoped<CacheCheckStep>();
        services.AddScoped<DownloadFileStep>();
        services.AddScoped<UploadToTelegramStep>();
        services.AddScoped<CacheAddStep>();

        services.AddScoped<CleanupStep>();

        return services;
    }
}