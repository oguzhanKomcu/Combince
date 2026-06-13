using Combince.Modules.Social.Infrastructure.Consumers;
using Combince.Shared.Core.Abstractions;
using Combince.Shared.Infrastructure.Messaging;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using StackExchange.Redis;
using ZiggyCreatures.Caching.Fusion; // Güncel namespace yapısı

namespace Combince.Shared.Infrastructure;

public static class ServiceRegistration
{
    public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {

        services.AddScoped<IEventBus, EventBus>();
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
            x.AddConsumer<UserRegisteredIntegrationConsumer>();
            x.AddConsumer<Combince.Modules.Users.Infrastructure.Consumers.UserFollowedConsumer>();
            x.AddConsumer<Combince.Modules.Users.Infrastructure.Consumers.UserUnfollowedConsumer>();
            // Tüm assembly'leri körü körüne taramak yerine, sadece "Combince" ile başlayan projeleri tara diyoruz
            x.AddConsumers(AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName != null && a.FullName.StartsWith("Combince"))
                .ToArray());

            x.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}