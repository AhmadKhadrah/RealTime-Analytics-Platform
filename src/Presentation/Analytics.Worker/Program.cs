// File: src/Presentation/Analytics.Worker/Program.cs
using Analytics.Infrastructure;
using Analytics.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        // تسجيل خدمات البنية التحتية وقراءة الإعدادات من appsettings.json الخاص بالـ Worker
        services.AddInfrastructureServices(hostContext.Configuration);
        
        // تسجيل الـ Hosted Service الخلفي
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();