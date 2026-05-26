using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MassTransit;
using MongoDB.Driver;
using StackExchange.Redis;
using ZiggyCreatures.Caching.Fusion; // Güncel namespace yapısı

namespace Combince.Shared.Infrastructure;

public static class ServiceRegistration
{
    public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. SQL Server Altyapısı
        var mssqlConnectionString = configuration.GetConnectionString("DefaultConnection");

        // 2. MongoDB Altyapısı
        var mongoConnectionString = configuration.GetConnectionString("MongoConnection");
        services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConnectionString));

        // 3. Redis ve FusionCache Altyapısı (L1/L2 Hibrit Önbellekleme)
        var redisConnectionString = configuration.GetConnectionString("RedisConnection");
        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            var multiplexer = ConnectionMultiplexer.Connect(redisConnectionString);
            services.AddSingleton<IConnectionMultiplexer>(multiplexer);

            // En güncel ve hatasız FusionCache konfigürasyon yapısı:
            services.AddFusionCache()
                .WithOptions(options =>
                {
                    // Varsayılan süre ayarı artık bu nesne üzerinden yönetiliyor
                    options.DefaultEntryOptions = new FusionCacheEntryOptions
                    {
                        Duration = TimeSpan.FromMinutes(2)
                    };
                })
                .WithNewtonsoftJsonSerializer(); // Doğrudan extension metot ile serileştiriciyi bağlıyoruz
        }

        // 4. MassTransit In-Memory Event Bus Ayarı
        services.AddMassTransit(x =>
        {
            x.AddConsumers(AppDomain.CurrentDomain.GetAssemblies());

            x.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}