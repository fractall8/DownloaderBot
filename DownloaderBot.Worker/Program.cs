using DownloaderBot.Shared.Configuration;
using DownloaderBot.Shared.Services;
using DownloaderBot.Worker;
using DownloaderBot.Worker.Services;

using StackExchange.Redis;

using Telegram.Bot;

var builder = Host.CreateApplicationBuilder(args);

var redisConnectionString = Environment.GetEnvironmentVariable("REDIS_CONNECTION") ?? "localhost";
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect($"{redisConnectionString},abortConnect=false"));

var botToken = Environment.GetEnvironmentVariable("BOT_TOKEN") ?? "token";

builder.Configuration.AddJsonFile("botsettings.json", optional: false, reloadOnChange: true);
builder.Services.Configure<BotSettings>(builder.Configuration.GetSection("BotSettings"));
builder.Services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(botToken));
builder.Services.AddSingleton<IBotResponseService, BotResponseService>();

builder.Services.AddSingleton<IDownloadProcessor, DownloadProcessor>();
builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton<IDownloaderService, YtDlpDownloaderService>();
builder.Services.AddSingleton<IUserQueueService, UserQueueService>();

var host = builder.Build();
host.Run();