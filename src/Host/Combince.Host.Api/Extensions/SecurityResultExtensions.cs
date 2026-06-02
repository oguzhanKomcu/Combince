using System.Text;
using Combince.Modules.Users.Core.Abstractions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Combince.Host.Api.Extensions;

public static class SecurityResultExtensions
{
    public static IServiceCollection AddSecurityInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        using var serviceProvider = services.BuildServiceProvider();
        var messageProvider = serviceProvider.GetRequiredService<ILocalizedMessageProvider>();

        var secretKey = configuration["Jwt:Key"] ?? throw new InvalidOperationException(messageProvider.GetUserMessage("JwtKeyNotFound"));
        var issuer = configuration["Jwt:Issuer"];
        var audience = configuration["Jwt:Audience"];

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
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization();

        return services;
    }
}