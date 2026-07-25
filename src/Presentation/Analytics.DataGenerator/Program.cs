// File: src/Presentation/Analytics.DataGenerator/Program.cs
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Analytics.Application.DTOs;
using Analytics.Domain.Enums;
using Analytics.Infrastructure.Services;
using Bogus;
using Microsoft.Extensions.Configuration;

class Program
{
    static async Task Main(string[] args)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("====================================================");
        Console.WriteLine("     🚀 STARTING REAL-TIME DATA GENERATOR 🚀         ");
        Console.WriteLine("====================================================");
        Console.ResetColor();

        // 1. بناء الإعدادات وقراءة ملف appsettings.json
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

        // 2. إنشاء كائن الـ Producer يدوياً (أو عبر DI إذا كان التطبيق أضخم)
        using var producer = new RabbitMQProducer(configuration);

        // 3. إعداد مكتبة Bogus لتوليد بيانات عشوائية ذكية ومترابطة
        var faker = new Faker();
        var random = new Random();

        // قائمة بصفحات الويب الافتراضية لمحاكاة تصفح المستخدمين
        var sampleUrls = new[] { "/home", "/products", "/products/details/123", "/cart", "/checkout", "/payment-success" };

        Console.WriteLine("\n[Press Ctrl+C to Stop Generating Events...]\n");

        int eventCounter = 0;

        while (true)
        {
            eventCounter++;

            // اختيار نوع حدث عشوائي
            var eventType = (EventType)random.Next(1, 6); // توليد قيمة بين 1 و 5 بناءً على الـ Enum

            // توليد قيمة مالية فقط إذا كان الحدث هو شراء (Purchase) أو إضافة للسلة (AddToCart)
            decimal value = 0;
            if (eventType == EventType.Purchase)
            {
                value = Math.Round((decimal)random.NextDouble() * 150 + 10, 2); // قيمة عشوائية بين 10$ و 160$
            }
            else if (eventType == EventType.AddToCart)
            {
                value = Math.Round((decimal)random.NextDouble() * 50 + 5, 2);
            }

            // توليد الـ DTO الخاص بالرسالة
            var eventDto = new EventMessageDto(
                UserId: faker.Internet.UserName(),
                Type: eventType,
                PageUrl: faker.PickRandom(sampleUrls),
                Value: value,
                Payload: $"{{\"browser\":\"{faker.Internet.UserAgent()}\", \"ip\":\"{faker.Internet.Ip()}\"}}",
                Timestamp: DateTime.UtcNow
            );

            try
            {
                // إرسال الرسالة إلى RabbitMQ
                await producer.PublishEventAsync(eventDto);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"[{DateTime.Now:HH:mm:ss}] ");
                Console.ResetColor();
                Console.WriteLine($"Sent Event #{eventCounter}: {eventDto.Type} | User: {eventDto.UserId} | Value: ${eventDto.Value}");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Error sending event #{eventCounter}: {ex.Message}");
                Console.ResetColor();
            }

            // الانتظار لمدة ثانية واحدة قبل توليد الحدث التالي (يمكنك تقليلها لمحاكاة ضغط أعلى)
            await Task.Delay(1000);
        }
    }
}