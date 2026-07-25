// File: src/Core/Analytics.Domain/Entities/SystemEvent.cs
using Analytics.Domain.Enums;

namespace Analytics.Domain.Entities;

public class SystemEvent
{
    public Guid Id { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public EventType Type { get; private set; }
    public string PageUrl { get; private set; } = string.Empty;
    public decimal Value { get; private set; } // في حال كان هناك قيمة مالية كالشراء مثلاً
    public string Payload { get; private set; } = string.Empty; // بيانات إضافية بصيغة JSON
    public DateTime CreatedAt { get; private set; }

    // مُعامل افتراضي مطلوب لـ Entity Framework
    private SystemEvent() { }

    // Factory Method لإنشاء الكائن بشكل آمن ومحكم
    public static SystemEvent Create(string userId, EventType type, string pageUrl, decimal value, string payload)
    {
        return new SystemEvent
        {
            Id = Guid.NewGuid(),
            UserId = string.IsNullOrEmpty(userId) ? "Anonymous" : userId,
            Type = type,
            PageUrl = pageUrl,
            Value = value,
            Payload = payload ?? string.Empty,
            CreatedAt = DateTime.UtcNow
        };
    }
}