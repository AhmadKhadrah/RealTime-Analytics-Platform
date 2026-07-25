// File: src/Presentation/Analytics.Api/Controllers/MetricsController.cs
using System;
using System.Threading.Tasks;
using Analytics.Application.Interfaces;
using Analytics.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Analytics.Api.Controllers;

[ApiController]
[Route("api/analytics")] // 👈 قمنا بتغييره هنا نصياً ليطابق طلب الـ Frontend: /api/analytics/...
public class MetricsController : ControllerBase
{
    private readonly ICacheService _cacheService;

    public MetricsController(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    // 👈 قمنا بإضافة الميثود الناقصة التي يستدعيها الـ Frontend عند التحميل أول مرة
    [HttpGet("live-metrics")]
    public async Task<IActionResult> GetLiveMetrics()
    {
        try
        {
            var metrics = await _cacheService.GetAllMetricsAsync();
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = "Failed to retrieve metrics", Details = ex.Message });
        }
    }

    [HttpGet("metric/{eventType}")]
    public async Task<IActionResult> GetMetric(string eventType)
    {
        // 1. محاولة تحويل النص القادم إلى الـ Enum الخاص بالحدث مع تجاهل حالة الأحرف (True)
        if (!Enum.TryParse<EventType>(eventType, true, out var parsedEventType))
        {
            return BadRequest(new { Error = $"The event type '{eventType}' is invalid." });
        }

        // 2. جلب البيانات الإحصائية الحالية مباشرة من كاش Redis
        var metric = await _cacheService.GetMetricAsync(parsedEventType);
        
        return Ok(metric);
    }
}