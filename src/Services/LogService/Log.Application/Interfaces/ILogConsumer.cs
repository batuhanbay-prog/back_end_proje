namespace Log.Application.Interfaces;

/// <summary>
/// RabbitMQ/Kafka'dan log mesajlarını dinleyen consumer interface.
/// </summary>
public interface ILogConsumer
{
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
}
