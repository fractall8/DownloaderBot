using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

var redisConnectionString = Environment.GetEnvironmentVariable("REDIS_CONNECTION") ?? "localhost";

builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(redisConnectionString));

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