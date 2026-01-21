using DownloaderBot.Shared.Configuration;
using DownloaderBot.Shared.Repositories;
using DownloaderBot.Shared.Services;
using DownloaderBot.Worker;
using DownloaderBot.Worker.Extensions;
using DownloaderBot.Worker.Services;

using Serilog;

using StackExchange.Redis;

using Telegram.Bot;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSerilog(config =>
    config.ReadFrom.Configuration(builder.Configuration));

var redisConnectionString = Environment.GetEnvironmentVariable("REDIS_CONNECTION") ?? "localhost";
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(redisConnectionString));

var botToken = Environment.GetEnvironmentVariable("BOT_TOKEN") ?? "token";

builder.Configuration.AddJsonFile("botsettings.json", optional: false, reloadOnChange: true);
builder.Services.Configure<BotSettings>(builder.Configuration.GetSection("BotSettings"));
builder.Services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(botToken));
builder.Services.AddSingleton<IBotResponseService, BotResponseService>();

builder.Services.AddPipelineSteps();
builder.Services.AddSingleton<IDownloadProcessor, DownloadProcessor>();

builder.Services.AddSingleton<RedisRepository>();
builder.Services.AddSingleton<ITaskRepository>(sp => sp.GetRequiredService<RedisRepository>());
builder.Services.AddSingleton<ICacheRepository>(sp => sp.GetRequiredService<RedisRepository>());
builder.Services.AddSingleton<IUserLimitRepository>(sp => sp.GetRequiredService<RedisRepository>());

builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton<IDownloaderService, YtDlpDownloaderService>();
builder.Services.AddSingleton<IUserQueueService, UserQueueService>();

var host = builder.Build();
host.Run();