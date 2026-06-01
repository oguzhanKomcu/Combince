using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Combince.Host.Api.Extensions;

/// <summary>
/// Uygulamanın güvenliğini sağlayan kimlik doğrulama (Authentication) ve 
/// yetkilendirme (Authorization) servislerinin kayıtlarını içeren genişletme sınıfı.
/// </summary>
public static class SecurityResultExtensions
{
    /// <summary>
    /// appsettings.json içerisindeki "Jwt" yapısını kullanarak JWT Bearer kimlik doğrulama şemasını sisteme kaydeder.
    /// </summary>
    /// <param name="services">Bağımlılık enjeksiyon konteyneri (<see cref="IServiceCollection"/>).</param>
    /// <param name="configuration">Uygulama yapılandırma ayarları (<see cref="IConfiguration"/>).</param>
    /// <returns>Zincirleme kullanım için güncellenmiş servis koleksiyonunu döner.</returns>
    /// <exception cref="InvalidOperationException">JWT Key tanımı appsettings.json içinde bulunamadığında fırlatılır.</exception>
    public static IServiceCollection AddSecurityInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Appsettings içindeki mevcut "Jwt" düğümünden verileri tam adlarıyla okuyoruz
        var secretKey = configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key tanımı appsettings içinde bulunamadı.");
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
                ClockSkew = TimeSpan.Zero // Token süresi bittiği saniye erişim kesilsin
            };
        });

        services.AddAuthorization();

        return services;
    }
}