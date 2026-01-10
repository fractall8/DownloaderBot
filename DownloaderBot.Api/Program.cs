using DownloaderBot.Api.Services;
using DownloaderBot.Shared.Configuration;
using DownloaderBot.Shared.Services;

using StackExchange.Redis;

using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

var redisConnectionString = Environment.GetEnvironmentVariable("REDIS_CONNECTION") ?? "localhost";

builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(redisConnectionString));

var botToken = Environment.GetEnvironmentVariable("BOT_TOKEN") ?? "token";

builder.Configuration.AddJsonFile("botsettings.json", optional: false, reloadOnChange: true);
builder.Services.Configure<BotSettings>(builder.Configuration.GetSection("BotSettings"));
builder.Services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(botToken));
builder.Services.AddScoped<IBotResponseService, BotResponseService>();
builder.Services.AddScoped<ILinkValidatorService, LinkValidatorService>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();