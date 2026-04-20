using EventManagement.Interfaces;
using EventManagement.Models.BookingModels;
using EventManagement.Models.Events;

namespace EventManagement.Services;

/// <summary>
/// Фоновый сервис обработки бронирований
/// </summary>
public class BookingHandlerService(ILogger<BackgroundService> logger, IBookingRepository repository, IEventRepository eventRepository) : BackgroundService
{
    private readonly ILogger<BackgroundService> _logger= logger;
    private readonly IBookingRepository _bookingRepository = repository;
    private readonly IEventRepository _eventRepository = eventRepository;
    private readonly SemaphoreSlim _processingSemaphor = new (1, 1);

    private const int ProcessingDelay = 2;
    private const int PollingInterval = 5;

    /// <summary>
    /// Метод сервиса фоновой обработки броней
    /// </summary>
    /// <param name="stoppingToken">Токен отмены</param>
    /// <returns>Пустая задача</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Фоновый сервис обработки бронирований запущен.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var tasks = _bookingRepository.GetPending().
                    Select(o => ProcessBookingAsync(o, stoppingToken));

                await Task.WhenAll(tasks);
                
                await Task.Delay(TimeSpan.FromSeconds(PollingInterval), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при бронировании события.");            
            }
        }

        _logger.LogInformation("Фоновый сервис обработки бронирований остановлен.");
    }

    private async Task ProcessBookingAsync(Booking booking, CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(ProcessingDelay), stoppingToken);

        await _processingSemaphor.WaitAsync(stoppingToken);

        Event? ev = null;
        try
        {
            ev = _eventRepository.GetByID(booking.EventId);
            if (ev == null)
            {
                booking.Reject();
                _logger.LogWarning($"Бронирование отклонено. Не найдено событие {booking.EventId} для брони {booking.Id}");
            }
            else
                booking.Confirm();            
            _bookingRepository.Update(booking);

            _logger.LogInformation($"Бронирование с id {booking.Id} обработано в {DateTimeOffset.UtcNow}.");
        }
        catch(Exception ex)
        {
            booking.Reject();
            if (ev != null)
            {                
                ev.ReleaseSeats(booking.SeatsCount);
                _eventRepository.Update(ev);
            }
            _bookingRepository.Update(booking);
            _logger.LogError(ex, $"Непредвиденная ошибка при обработке бронирования id {booking.Id}");
        }
        finally
        {
            _processingSemaphor.Release();
        }
    }
}