// File: src/Core/Analytics.Application/Interfaces/IAnalyticsHubService.cs
using Analytics.Application.DTOs;

namespace Analytics.Application.Interfaces;

public interface IAnalyticsHubService
{
    Task BroadcastMetricUpdateAsync(RealTimeMetricDto metric);
}