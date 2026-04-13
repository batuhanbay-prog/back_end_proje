namespace Product.Application.Interfaces;

/// <summary>
/// Event bus soyutlaması — MassTransit implementasyonu ADIM 7'de Product.Infrastructure'da yapılır.
/// </summary>
public interface IEventBus
{
    Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class;
}
