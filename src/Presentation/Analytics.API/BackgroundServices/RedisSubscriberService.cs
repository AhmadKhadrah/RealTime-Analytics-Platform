// File: src/Presentation/Analytics.Api/BackgroundServices/RedisSubscriberService.cs
using Analytics.Api.Hubs;
using Analytics.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Threading;
using System.Threading.Tasks;

namespace Analytics.Api.BackgroundServices;

public class RedisSubscriberService : BackgroundService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IHubContext<AnalyticsHub> _hubContext;
    private readonly ILogger<RedisSubscriberService> _logger;

    public RedisSubscriberService(
        IConnectionMultiplexer redis, 
        IHubContext<AnalyticsHub> hubContext,
        ILogger<RedisSubscriberService> logger)
    {
        _redis = redis;
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var subscriber = _redis.GetSubscriber();

        // الاشتراك في القناة التي ينشر فيها الـ Worker
        subscriber.Subscribe(RedisChannel.Literal("analytics_updates"), async (channel, message) =>
        {
            _logger.LogInformation($"📢 Redis Pub/Sub received update for: {message}");

            // بث التحديث لحظياً لجميع واجهات المستخدم المتصلة بـ SignalR
            await _hubContext.Clients.All.SendAsync("ReceiveMetricsUpdate", message.ToString(), stoppingToken);
        });

        return Task.CompletedTask;
    }
}