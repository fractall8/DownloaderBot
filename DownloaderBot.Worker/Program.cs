using DownloaderBot.Worker;
using DownloaderBot.Worker.Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton<IDownloaderService, YtDlpWrapper>();

var host = builder.Build();
host.Run();