// File: src/Presentation/Analytics.Api/Hubs/AnalyticsHub.cs
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Analytics.Api.Hubs;

public class AnalyticsHub : Hub
{
    // عند اتصال واجهة المستخدم (Frontend)، يمكننا إرسال رسالة ترحيبية أو ربطه بغرفة معينة
    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("WelcomeMessage", "Connected to Real-Time Analytics Hub!");
        await base.OnConnectedAsync();
    }
}