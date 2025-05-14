// Program.cs
// Entry point for the SignalR server application.
// Configures services, middleware, authentication, SignalR, and logging.

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Serilog;
using StackExchange.Redis;

/// <summary>
/// The main program class for the SignalR server application.
/// Configures logging, services, authentication, SignalR, and middleware pipeline.
/// </summary>
public class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    public static async Task Main(string[] args)
    {
        // Configure Serilog logging
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
            .Enrich.FromLogContext()
            .CreateLogger();

        var builder = WebApplication.CreateBuilder(args);
        builder.Host.UseSerilog();

        // Add in-memory caching
        builder.Services.AddMemoryCache();

        // Configure Entity Framework Core with SQLite
        builder.Services.AddDbContext<Entities.ApplicationDbContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

        // Configure ASP.NET Core Identity
        builder.Services.AddIdentity<Entities.Models.User, IdentityRole<Guid>>()
            .AddEntityFrameworkStores<Entities.ApplicationDbContext>()
            .AddDefaultTokenProviders();

        // Configure JWT authentication
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

        // Add controllers and Swagger/OpenAPI
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Register application services
        builder.Services.AddScoped<Shared.Services.Interfaces.IVerificationService, Shared.Services.VerificationService>();
        builder.Services.AddScoped<Shared.Services.Interfaces.IVerificationCodeStore, Shared.Services.InMemoryVerificationCodeStore>();

        // Configure SignalR with Redis backplane
        builder.Services.AddSignalR(hubOptions =>
        {
            hubOptions.EnableDetailedErrors = true;
            hubOptions.KeepAliveInterval = TimeSpan.FromMinutes(1);
        })
        .AddStackExchangeRedis("localhost:6379", options =>
        {
            options.Configuration.ChannelPrefix = RedisChannel.Literal("Rasaaill");
        });

        // Configure email sender service based on environment
        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddScoped<Shared.Services.Interfaces.IEmailSender, Shared.Services.LocalEmailSender>();
        }
        else
        {
            builder.Services.AddScoped<Shared.Services.Interfaces.IEmailSender, Shared.Services.SendGridEmailSender>();
        }

        // Register hosted background services
        builder.Services.AddHostedService<Shared.Services.DiscoveryService>();

        // Add OpenAPI support
        builder.Services.AddOpenApi();

        var app = builder.Build();

        // Seed initial user data
        await Entities.Seeders.UserSeeder.SeedAsync(app.Services);

        // Configure HTTP request pipeline
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Serve static files from wwwroot under /media path
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(
                Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")),
            RequestPath = "/media"
        });

        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        // Map SignalR hub endpoint
        app.MapHub<Server.SignalR.Services.ChatHub>("/chatHub");

        app.Run();
    }
}
