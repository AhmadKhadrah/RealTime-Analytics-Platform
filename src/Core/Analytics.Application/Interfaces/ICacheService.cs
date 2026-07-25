// File: src/Core/Analytics.Application/Interfaces/ICacheService.cs
using Analytics.Application.DTOs;
using Analytics.Domain.Enums;

namespace Analytics.Application.Interfaces;

public interface ICacheService
{
    // لزيادة العدادات والقيمة المالية لحدث معين لحظياً في Redis
    Task IncrementMetricAsync(EventType eventType, decimal value);

    // لجلب الإحصائيات الحالية مباشرة لعرضها في لوحة التحكم
    Task<RealTimeMetricDto> GetMetricAsync(EventType eventType);

    // لجلب كل الإحصائيات لجميع أنواع الأحداث دفعة واحدة
    Task<IEnumerable<RealTimeMetricDto>> GetAllMetricsAsync();
    // أضف هذا السطر داخل واجهة ICacheService
    Task PublishUpdateAsync(string channel, string message);
}