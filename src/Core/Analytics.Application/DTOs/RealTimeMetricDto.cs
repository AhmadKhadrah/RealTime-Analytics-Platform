// File: src/Core/Analytics.Application/DTOs/RealTimeMetricDto.cs
namespace Analytics.Application.DTOs;

public record RealTimeMetricDto(
    string EventType,
    int TotalCount,
    decimal TotalValue,
    DateTime LastUpdatedUtc
);