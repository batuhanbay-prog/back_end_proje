using MassTransit;
using Product.Application.Interfaces;

namespace Product.Infrastructure.Messaging;

/// <summary>
/// IEventBus'ın MassTransit + RabbitMQ implementasyonu.
/// Ürün ekleme/güncelleme event'lerini RabbitMQ'ya publish eder.
/// </summary>
public class MassTransitEventBus : IEventBus
{
    private readonly IPublishEndpoint _publishEndpoint;

    public MassTransitEventBus(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
    {
        await _publishEndpoint.Publish(message, cancellationToken);
    }
}
