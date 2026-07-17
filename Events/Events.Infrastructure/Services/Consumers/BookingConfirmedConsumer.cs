using Confluent.Kafka;
using Contracts;
using Events.Application.Exceptions;
using Events.Application.Interfaces.Consumers;
using Events.Infrastructure.Settings.ConsumerSettings;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Events.Infrastructure.Services.Consumers;

/// <summary>
/// Консьюмер для обработки BookingConfirmed сообщений
/// </summary>
public class BookingConfirmedConsumer : IBookingConfirmedConsumer
{
    private readonly IConsumer<string, string> _consumer;

    /// <summary>
    /// Консруктор
    /// </summary>
    /// <param name="options">Настройки</param>
    /// <exception cref="ArgumentNullException">Если настройки не заданы</exception>
    public BookingConfirmedConsumer(IOptions<BookingConfirmedConsumerSettings> options)
    {
        var settings = options.Value ?? throw new ArgumentNullException("Не заданы настройки консьюмера 'BookingConfirmedConsumer'");
        if (string.IsNullOrEmpty(settings.BootstrapServers)
            || string.IsNullOrEmpty(settings.GroupId)
            || string.IsNullOrEmpty(settings.Topic))
            throw new ArgumentNullException("Неверно заданы настройки консьюмера 'BookingConfirmedConsumer'");

        var cfg = new ConsumerConfig()
        {
            BootstrapServers = settings.BootstrapServers,
            GroupId = settings.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            EnableAutoOffsetStore = false,
            AllowAutoCreateTopics = true

        };

        _consumer = new ConsumerBuilder<string, string>(cfg).Build();
        _consumer.Subscribe(settings.Topic);
    }

    ///<inheritdoc/>
    public async Task ConsumeAsync(Func<BookingConfirmed, CancellationToken, Task> messageHandler, CancellationToken token)
    {
        try
        {
            
            var result = _consumer.Consume(token);
            if (result == null)
            {
                return;
            }

            var bookingConfirmed = JsonSerializer.Deserialize<BookingConfirmed>(result.Message.Value);
            if (bookingConfirmed == null)
                throw new InvalidOperationException("Ошибка при десериализации ссобщения Kafka.");

            await messageHandler(bookingConfirmed, token);

            _consumer.StoreOffset(result);
            _consumer.Commit();
        }
        catch (ConsumeException ex) 
        {
            if(ex.Error.IsFatal)
            {
                //ToDo: тут надо как-то перебилдить консьюмер
            }

            throw new ConsumerException($"Ошибка получения сообщения из Kafka: {ex.Error.Reason}");
        }        
    } 
    /// <summary>
    /// Очистка ресурсов
    /// </summary>
    public void Dispose()
    {
        _consumer?.Close();
        _consumer?.Dispose();
    }
}
