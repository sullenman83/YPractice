using Bookings.Application.Interfaces.Consumers;
using Confluent.Kafka;

namespace Bookings.Infrastructure.Services.Consumers;

public class EventSeatsReservedConsumer : IEventSeatsReservedConsumer, IDisposable
{
    private readonly IConsumer<string, string> _consumer;

    public Task ConsumeAsync()
    {
        
    }

    /// <summary>
    /// Очистить ресурсы
    /// </summary>
    public void Dispose()
    {
        _consumer?.Close();
        _consumer?.Dispose();
    }
}
