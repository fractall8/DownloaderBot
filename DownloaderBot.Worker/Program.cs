using DownloaderBot.Worker;
using DownloaderBot.Worker.Services;
using StackExchange.Redis;
using Telegram.Bot;

var builder = Host.CreateApplicationBuilder(args);
var redisConnectionString = Environment.GetEnvironmentVariable("REDIS_CONNECTION") ?? "localhost";
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect($"{redisConnectionString},abortConnect=false"));

var botToken = Environment.GetEnvironmentVariable("BOT_TOKEN") ?? "token";
builder.Services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(botToken));

builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton<IDownloaderService, YtDlpWrapper>();

var host = builder.Build();
host.Run();