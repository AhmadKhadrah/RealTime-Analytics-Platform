// File: src/Infrastructure/Analytics.Infrastructure/Services/RabbitMQProducer.cs
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Analytics.Application.DTOs;
using Analytics.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace Analytics.Infrastructure.Services;

public class RabbitMQProducer : IMessageProducer, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel; // يعود لاستخدام IModel المستقرة في v6
    private const string ExchangeName = "analytics_exchange";
    private const string QueueName = "analytics_events_queue";
    private const string RoutingKey = "analytics.event.created";

    public RabbitMQProducer(IConfiguration configuration)
    {
        var hostName = configuration["RabbitMQ:HostName"] ?? "localhost";
        var userName = configuration["RabbitMQ:UserName"] ?? "guest";
        var password = configuration["RabbitMQ:Password"] ?? "guest";

        var factory = new ConnectionFactory
        {
            HostName = hostName,
            UserName = userName,
            Password = password
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel(); // إنشاء القناة بالنمط القياسي

        // إعداد الـ Exchange والـ Queue والـ Binding
        _channel.ExchangeDeclare(exchange: ExchangeName, type: ExchangeType.Direct, durable: true);
        _channel.QueueDeclare(queue: QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueBind(queue: QueueName, exchange: ExchangeName, routingKey: RoutingKey);
    }

    public Task PublishEventAsync(EventMessageDto eventMessage)
    {
        var jsonPayload = JsonSerializer.Serialize(eventMessage);
        var body = Encoding.UTF8.GetBytes(jsonPayload);

        var properties = _channel.CreateBasicProperties();
        properties.Persistent = true; // لضمان عدم ضياع الرسائل عند ريستارت السيرفر

        _channel.BasicPublish(
            exchange: ExchangeName,
            routingKey: RoutingKey,
            basicProperties: properties,
            body: body
        );

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (_channel != null && _channel.IsOpen) _channel.Close();
        if (_connection != null && _connection.IsOpen) _connection.Close();
    }
}