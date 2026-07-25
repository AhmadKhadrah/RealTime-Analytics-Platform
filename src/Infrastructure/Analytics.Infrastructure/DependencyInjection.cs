// File: src/Infrastructure/Analytics.Infrastructure/DependencyInjection.cs
using Analytics.Application.Interfaces;
using Analytics.Infrastructure.Persistence;
using Analytics.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Analytics.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
{
    // 1. إعداد اتصال SQL Server
    var sqlConnectionString = configuration.GetConnectionString("DefaultConnection");
    services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(sqlConnectionString, b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

    // 2. إعداد اتصال Redis
    var redisConnectionString = configuration.GetConnectionString("RedisConnection") ?? "localhost:6379";
    services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));

    // 3. تسجيل الـ Cache Service
    services.AddScoped<ICacheService, RedisCacheService>();

    // 4. تسجيل RabbitMQ Producer (السطر الجديد)
    services.AddSingleton<IMessageProducer, RabbitMQProducer>();

    return services;
}
}