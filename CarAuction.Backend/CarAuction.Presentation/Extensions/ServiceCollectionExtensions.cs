using CarAuction.Application.Interfaces.Caching;
using CarAuction.Application.Interfaces.Repositories;
using CarAuction.Application.Interfaces.Services;
using CarAuction.Application.Services;
using CarAuction.Infrastructure.Caching;
using CarAuction.Infrastructure.Data.Connection;
using CarAuction.Infrastructure.Repositories;
using CarAuction.Infrastructure.Security;
using CarAuction.Presentation.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Text;

namespace CarAuction.Presentation.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Connection string
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Host=localhost;Database=carauction_db;Username=postgres;Password=postgres";

        // Register connection factory
        services.AddSingleton<IConnectionFactory>(new ConnectionFactory(connectionString));

        // Register Redis Connection and Caching Service
        var redisConnString = configuration.GetConnectionString("Redis") 
            ?? configuration["ConnectionStrings:Redis"] 
            ?? "localhost:6379";

        try
        {
            var configOptions = ConfigurationOptions.Parse(redisConnString);
            configOptions.AbortOnConnectFail = false;
            configOptions.ConnectTimeout = 2000;
            var redis = ConnectionMultiplexer.Connect(configOptions);
            services.AddSingleton<IConnectionMultiplexer>(redis);
        }
        catch
        {
            // Redis unreachable in local disconnected dev - RedisCacheService gracefully handles fallback
        }

        services.AddSingleton<ICacheService, RedisCacheService>();

        // Register Notification Service (SignalR Real-Time Broadcaster)
        services.AddScoped<INotificationService, NotificationService>();

        // Register repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IListingRepository, ListingRepository>();
        services.AddScoped<IAuctionRepository, AuctionRepository>();
        services.AddScoped<IBidsRepository, BidsRepository>();
        services.AddScoped<IImageRepository, ImageRepository>();
        services.AddScoped<IGroupRepository, GroupRepository>();
        services.AddScoped<IDirectMessageRepository, DirectMessageRepository>();

        // Register security services
        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        var jwtSecretKey = configuration["Jwt:SecretKey"] ?? "your_super_secret_jwt_key_at_least_64_characters_long_123456";
        var jwtIssuer = configuration["Jwt:Issuer"] ?? "CarAuction";
        var jwtAudience = configuration["Jwt:Audience"] ?? "CarAuction";
        var jwtExpiration = int.Parse(configuration["Jwt:ExpirationMinutes"] ?? "60");

        services.AddSingleton<ITokenService>(new JwtTokenService(jwtSecretKey, jwtIssuer, jwtAudience, jwtExpiration));

        // Register application services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRolesPermissionsService, RolesPermissionsService>();
        services.AddScoped<IListingService, ListingService>();
        services.AddScoped<IAuctionService, AuctionService>();
        services.AddScoped<IBidsService, BidsService>();
        services.AddScoped<IImageService, ImageService>();
        services.AddScoped<IGroupService, GroupService>();
        services.AddScoped<IDirectMessageService, DirectMessageService>();

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSecretKey = configuration["Jwt:SecretKey"] ?? "your_super_secret_jwt_key_at_least_64_characters_long_123456";
        var jwtIssuer = configuration["Jwt:Issuer"] ?? "CarAuction";
        var jwtAudience = configuration["Jwt:Audience"] ?? "CarAuction";

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
                ClockSkew = TimeSpan.Zero
            };

            // Support JWT access token from query string for SignalR WebSocket connections
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                    {
                        context.Token = accessToken;
                    }
                    return Task.CompletedTask;
                }
            };
        });

        return services;
    }
}
