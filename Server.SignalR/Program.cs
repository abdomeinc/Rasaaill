using Serilog;
using StackExchange.Redis;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .CreateLogger();


var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// Add services to the container.

// SignalR Configuration
builder.Services.AddSignalR(hubOptions => {
    hubOptions.EnableDetailedErrors = true;
    hubOptions.KeepAliveInterval = TimeSpan.FromMinutes(1);
})
.AddStackExchangeRedis("localhost:6379", options =>
{
    options.Configuration.ChannelPrefix = RedisChannel.Literal("Rasaaill");
});
    ;

builder.Services.AddHostedService<Shared.Services.DiscoveryService>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapHub<Server.SignalR.Services.ChatHub>("/chatHub");

app.Run();
