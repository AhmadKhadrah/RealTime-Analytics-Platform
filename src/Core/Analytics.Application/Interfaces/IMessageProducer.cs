// File: src/Core/Analytics.Application/Interfaces/IMessageProducer.cs
using Analytics.Application.DTOs;

namespace Analytics.Application.Interfaces;

public interface IMessageProducer
{
    Task PublishEventAsync(EventMessageDto eventMessage);
}