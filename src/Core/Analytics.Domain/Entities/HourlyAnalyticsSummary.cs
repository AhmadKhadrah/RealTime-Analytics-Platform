// File: src/Core/Analytics.Domain/Entities/HourlyAnalyticsSummary.cs
using Analytics.Domain.Enums;

namespace Analytics.Domain.Entities;

public class HourlyAnalyticsSummary
{
    public Guid Id { get; private set; }
    public DateTime HourUtc { get; private set; } // بداية الساعة (مثلاً 2026-07-13 16:00:00)
    public EventType Type { get; private set; }
    public int TotalCount { get; private set; }
    public decimal TotalValue { get; private set; }

    private HourlyAnalyticsSummary() { }

    public static HourlyAnalyticsSummary Create(DateTime hourUtc, EventType type)
    {
        return new HourlyAnalyticsSummary
        {
            Id = Guid.NewGuid(),
            HourUtc = new DateTime(hourUtc.Year, hourUtc.Month, hourUtc.Day, hourUtc.Hour, 0, 0, DateTimeKind.Utc),
            Type = type,
            TotalCount = 0,
            TotalValue = 0
        };
    }

    // ميثود لتحديث القيم التراكمية بشكل آمن داخل الـ Domain
    public void AddEvent(decimal value)
    {
        TotalCount++;
        TotalValue += value;
    }
}