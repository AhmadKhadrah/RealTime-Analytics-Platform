// File: src/Presentation/Analytics.Api/Program.cs
using Analytics.Api.BackgroundServices;
using Analytics.Api.Hubs;
using Analytics.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// 1. تسجيل خدمات الـ Controller والـ SignalR
builder.Services.AddSignalR();
builder.Services.AddControllers();


// 2. تسجيل خدمات البنية التحتية (SQL, Redis, Connection Multiplexer)
builder.Services.AddInfrastructureServices(builder.Configuration);

// 3. تسجيل الـ Redis Subscriber الخدمي للاستماع للتحديثات في الخلفية
builder.Services.AddHostedService<RedisSubscriberService>();

// 4. إعداد الـ CORS لكي نسمح للـ Frontend بالاتصال بالـ API و SignalR Hub دون قيود أمنية أثناء التطوير
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173") // منافذ الـ React/Vite الشائعة
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // مطلوبة لعمل الـ WebSockets/SignalR
    });
});

var app = builder.Build();

// تفعيل إعدادات الـ Middleware
app.UseRouting();
app.UseCors();
app.UseAuthorization();
// اجعلها تعمل فقط في غير بيئة التطوير، أو احذف السطر تماماً للتطوير المحلي
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// رسم مسارات الـ Controllers والـ SignalR Hub
app.MapControllers();
app.MapHub<AnalyticsHub>("/analyticsHub");

app.Run();