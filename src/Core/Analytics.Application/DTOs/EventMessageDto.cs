// File: src/Core/Analytics.Application/DTOs/EventMessageDto.cs
using Analytics.Domain.Enums;

namespace Analytics.Application.DTOs;

// هذا الكائن يمثل شكل الرسالة التي يستقبلها RabbitMQ من محاكي البيانات
public record EventMessageDto(
    string UserId,
    Analytics.Domain.Enums.EventType Type,
    string PageUrl,
    decimal Value,
    string Payload,
    DateTime Timestamp
);