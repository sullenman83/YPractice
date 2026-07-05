using Bookings.Application.AppSettings;
using Bookings.Application.Exceptions;
using Bookings.Application.Interfaces;
using Bookings.Application.Interfaces.Repositories;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bookings.Application.Services.BackgrounServices.Producers;

/// <summary>
/// Фоновый сервис для отправки сообщений в Kafka
/// </summary>
public class BackgroundProducerService: BackgroundService
{
    private readonly ILogger<BackgroundProducerService> _logger;
    private readonly IBookingProduсer _bookingProducer;
    private readonly BackgroundProducerServiceSettings _settings;
    private readonly IOutboxMessageRepository _repository;


    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="logger">Логер</param>
    /// <param name="options">Натсройки сервиса</param>    
    /// <param name="bookingProducer">Продюсер</param>    
    /// <param name="repository">Репозиторий сообщений outbox</param>    
    public BackgroundProducerService(ILogger<BackgroundProducerService> logger
        , IOptions<BackgroundProducerServiceSettings> options
        , IBookingProduсer bookingProducer
        , IOutboxMessageRepository repository)
    {
        _logger = logger;        
        _settings = options.Value ?? throw new ArgumentNullException("Не заданы настройки для фонового сервиса отправки сообщений в Kafka.");
        _bookingProducer = bookingProducer;
        _repository = repository;
    }

    /// <summary>
    /// Метод запуска сервиса
    /// </summary>
    /// <param name="stoppingToken">Токен отмены</param>
    /// <returns>Пустая задача</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Фоновый сервис BookingProducer запущен.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var messages = await _repository.GetUnprocessed(stoppingToken);

                foreach (var m in messages)
                {
                    try
                    {
                        if (m.RetryCount >= _settings.MaxRetryCount)
                        {
                            //ToDO: тут что-то надо сделать с этим сообщением. Оменить бронь и удалить сообщений или поместить сообщения в отдельный топик
                            _logger.LogCritical($"Достигнуто максимальное количество повторных отправлений смообщения {m.MessageType}, {m.OccuredOn}, {m.Payload}");
                            break;
                        }

                        var topic = _settings.Topics[m.MessageType];
                        await _bookingProducer.ProduceAsync(topic, m.Key.ToString(), m.Payload);
                        m.Processed = true;
                        await _repository.SaveChangesAsync();
                    }
                    catch (BookingProducerException ex)
                    {
                        _logger.LogError(ex.Message);                        
                        m.RetryCount = m.RetryCount + 1;
                        await _repository.SaveChangesAsync();
                        break;
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при бронировании события.");            
            }
            finally
            {
                await Task.Delay(TimeSpan.FromMilliseconds(_settings.PollingInterval), stoppingToken);
            }
        }

        _logger.LogInformation("Фоновый сервис обработки бронирований остановлен.");
    }
}