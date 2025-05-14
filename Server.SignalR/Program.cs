using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
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
builder.Services.AddMemoryCache();
builder.Services.AddDbContext<Entities.ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<Entities.Models.User, IdentityRole<Guid>>()
    .AddEntityFrameworkStores<Entities.ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"] ?? ""))
        };
    });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



// SignalR Configuration
builder.Services.AddSignalR(hubOptions =>
{
    hubOptions.EnableDetailedErrors = true;
    hubOptions.KeepAliveInterval = TimeSpan.FromMinutes(1);
})
.AddStackExchangeRedis("localhost:6379", options =>
{
    options.Configuration.ChannelPrefix = RedisChannel.Literal("Rasaaill");
});

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddScoped<Shared.Services.Interfaces.IEmailSender, Shared.Services.LocalEmailSender>();
}
else
{
    builder.Services.AddScoped<Shared.Services.Interfaces.IEmailSender, Shared.Services.SendGridEmailSender>();
}


builder.Services.AddHostedService<Shared.Services.DiscoveryService>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

await Entities.Seeders.UserSeeder.SeedAsync(app.Services);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwagger();
    app.UseSwaggerUI();
}
//app.UseStaticFiles(); // This serves files from wwwroot
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")),
    RequestPath = "/media" // So files are accessed via /media/filename.ext
});


app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapHub<Server.SignalR.Services.ChatHub>("/chatHub");

app.Run();
