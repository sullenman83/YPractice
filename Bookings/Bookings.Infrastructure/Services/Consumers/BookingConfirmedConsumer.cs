using Bookings.Application.Interfaces.Consumers;
using Bookings.Infrastructure.Data.Configurations;
using Bookings.Infrastructure.Settings.ConsumersSettings;
using Confluent.Kafka;
using Contracts;
using Microsoft.Extensions.Options;

namespace Bookings.Infrastructure.Services.Consumers;

internal class BookingConfirmedConsumer : IBookingConfirmedConsumer
{
    private readonly IConsumer<string, string> _consumer;

    public BookingConfirmedConsumer(IOptions<BookingConfirmedConsumerSettings> options)
    {
        var settings = options.Value ?? throw new ArgumentNullException("Не заданы настройки консьюмера 'EventSeatsReservedConsumer'");
        if (string.IsNullOrEmpty(settings.BootstrapServers)
            || string.IsNullOrEmpty(settings.GroupId)
            || string.IsNullOrEmpty(settings.Topic))
            throw new ArgumentNullException("Неверно заданы настройки консьюмера 'EventSeatsReservedConsumer'");

        var cfg = new ConsumerConfig()
        {
            BootstrapServers = settings.BootstrapServers,
            GroupId = settings.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            EnableAutoOffsetStore = false
        };

        _consumer = new ConsumerBuilder<string, string>(cfg).Build();
        _consumer.Subscribe(settings.Topic);
    }

    public void Consume(Action<BookingConfirmed> messageHandler, CancellationToken token)
    {
        try
        {
            _consumer.Consume()
        }
        catch (Exception ex) 
        {
        }
        
    }

    public void Dispose()
    {
        _consumer?.Close();
        _consumer?.Dispose();
    }
}
