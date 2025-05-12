
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// SignalR Configuration
builder.Services.AddSignalR(hubOptions => {
    hubOptions.EnableDetailedErrors = true;
    hubOptions.KeepAliveInterval = TimeSpan.FromMinutes(1);
});

// Add Services
builder.Services.AddSingleton<ChatServer.Services.Interfaces.IFirebaseService, ChatServer.Services.FirebaseService>();
//builder.Services.AddHostedService<ChatServer.Services.Interfaces.IDiscoveryService, ChatServer.Services.DiscoveryService>();
builder.Services.AddHostedService<ChatServer.Services.DiscoveryService>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
// 🔥 Map the SignalR hub AFTER services are registered

app.MapHub<ChatServer.SignalR.ChatHub>("/chatHub");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
