// File: src/Infrastructure/Analytics.Infrastructure/Services/RedisCacheService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Analytics.Application.DTOs;
using Analytics.Application.Interfaces;
using Analytics.Domain.Enums;
using StackExchange.Redis;

namespace Analytics.Infrastructure.Services;

public class RedisCacheService : ICacheService
{
    private readonly IDatabase _database;
    private readonly IConnectionMultiplexer _redis; // سنحتاجها لاستخدام Pub/Sub إذا لزم الأمر

    public RedisCacheService(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _database = redis.GetDatabase();
    }

    public async Task IncrementMetricAsync(EventType eventType, decimal value)
    {
        var keyPrefix = $"analytics:{eventType.ToString().ToLower()}";

        // 1. زيادة العداد الكلي للحدث بمقدار 1
        await _database.StringIncrementAsync($"{keyPrefix}:count");

        // 2. زيادة القيمة المالية أو المقياس التراكمي (إذا كانت القيمة أكبر من صفر)
        if (value > 0)
        {
            await _database.StringIncrementAsync($"{keyPrefix}:value", (double)value);
        }
    }

    public async Task<int> GetMetricCountAsync(EventType eventType)
    {
        var key = $"analytics:{eventType.ToString().ToLower()}:count";
        var value = await _database.StringGetAsync(key);
        
        return value.HasValue ? (int)value : 0;
    }

    public async Task<decimal> GetMetricValueAsync(EventType eventType)
    {
        var key = $"analytics:{eventType.ToString().ToLower()}:value";
        var value = await _database.StringGetAsync(key);
        
        return value.HasValue ? (decimal)(double)value : 0;
    }

    // 👈 1. حل مشكلة ICacheService.GetMetricAsync
    public async Task<RealTimeMetricDto> GetMetricAsync(EventType eventType)
    {
        var count = await GetMetricCountAsync(eventType);
        var value = await GetMetricValueAsync(eventType);

        return new RealTimeMetricDto(
            EventType: eventType.ToString(),
            TotalCount: count,
            TotalValue: value,
            LastUpdatedUtc: DateTime.UtcNow
        );
    }

    // 👈 2. حل مشكلة ICacheService.GetAllMetricsAsync
    public async Task<IEnumerable<RealTimeMetricDto>> GetAllMetricsAsync()
    {
        var metrics = new List<RealTimeMetricDto>();
        var eventTypes = Enum.GetValues<EventType>();

        foreach (var type in eventTypes)
        {
            var metric = await GetMetricAsync(type);
            metrics.Add(metric);
        }

        return metrics;
    }

    // 👈 3. حل مشكلة ICacheService.PublishUpdateAsync (تُستخدم للـ Redis Pub/Sub الداخلي عند الحاجة للربط الأفقي للأجهزة)
    public async Task PublishUpdateAsync(string channel, string message)
    {
        var subscriber = _redis.GetSubscriber();
        await subscriber.PublishAsync(RedisChannel.Literal(channel), message);
    }
}