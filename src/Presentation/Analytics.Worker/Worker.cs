// File: src/Presentation/Analytics.Worker/Worker.cs
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Analytics.Application.DTOs;
using Analytics.Application.Interfaces;
using Analytics.Domain.Entities;
using Analytics.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Analytics.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private IConnection _connection;
    private IModel _channel;
    private const string QueueName = "analytics_events_queue";

    public Worker(ILogger<Worker> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        InitRabbitMQ();
    }

    private void InitRabbitMQ()
    {
        // تهيئة الاتصال بـ RabbitMQ بنفس الإعدادات القياسية
        var factory = new ConnectionFactory { HostName = "localhost" };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        
        // نضمن أن الطابور موجود
        _channel.QueueDeclare(queue: QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        
        // تحديد كمية الرسائل المستلمة لكل دورة (Prefetch Count) لتحسين الأداء وتوزيع الضغط
        _channel.BasicQos(prefetchSize: 0, prefetchCount: 10, global: false);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        var consumer = new EventingBasicConsumer(_channel);
        
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            
            try
            {
                // 1. القراءة وتحويل الـ JSON إلى DTO
                var eventDto = JsonSerializer.Deserialize<EventMessageDto>(message);
                
                if (eventDto != null)
                {
                    _logger.LogInformation($"📥 Received Event: {eventDto.Type} from User: {eventDto.UserId}");

                    // 2. إنشاء Scope للتعامل مع خدمات الـ Scoped (مثل Database و Cache)
                    using var scope = _scopeFactory.CreateScope();
                    
                    var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    // 3. تحديث Redis لحظياً (عملية سريعة جداً)
                    await cacheService.IncrementMetricAsync(eventDto.Type, eventDto.Value);

                    // 4. حفظ الحدث في SQL Server
                    var systemEvent = SystemEvent.Create(
                        eventDto.UserId,
                        eventDto.Type,
                        eventDto.PageUrl,
                        eventDto.Value,
                        eventDto.Payload
                    );
                    
                    dbContext.SystemEvents.Add(systemEvent);
                    await dbContext.SaveChangesAsync(stoppingToken);
                    // داخل ملف Worker.cs - بعد dbContext.SaveChangesAsync
                    // قم بنشر رسالة تحديث تخبر الـ API بنوع الحدث الذي تم تحديثه
                    await cacheService.PublishUpdateAsync("analytics_updates", eventDto.Type.ToString());
                }

                // 5. تأكيد استلام ومعالجة الرسالة بنجاح لـ RabbitMQ لحذفها من الطابور (Acknowledgment)
                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error processing message: {ex.Message}");
                // في حال حدوث خطأ، نعيد الرسالة للطابور ليتم محاولة معالجتها مجدداً (Re-queue)
                _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
            }
        };

        _channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        if (_channel != null && _channel.IsOpen) _channel.Close();
        if (_connection != null && _connection.IsOpen) _connection.Close();
        base.Dispose();
    }
}