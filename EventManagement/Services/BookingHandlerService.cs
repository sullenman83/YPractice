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
                var bookings = _bookingRepository.Bookings
                    .Where(o => o.Value.Status == BookingStatus.Pending)
                    .Take(5);

                foreach (var booking in bookings)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);

                    var isConfirmed = Random.Shared.Next(0, 3) > 0 ? true : false;
                    var b = booking.Value.Clone();
                    b.Status = isConfirmed ? BookingStatus.Confirmed : BookingStatus.Rejected;
                    b.ProcessedAt = DateTime.Now;
                    _bookingRepository.Bookings.TryUpdate(booking.Key, b, booking.Value);

                    _logger.LogInformation($"Бронирование с id {booking.Key} обработано.");                   
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
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
        await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);

        await _processingSemaphor.WaitAsync(stoppingToken);

        Event? ev = null;
        try
        {
            ev = _eventRepository.GetByID(booking.EventId);
            if (ev == null)
            {
                booking.Reject();
                _logger.LogWarning($"Ошибка при обработке бронирования. Не найдено событие {booking.EventId} для брони {booking.Id}");
            }
            else
                booking.Confirm();
            _bookingRepository.Update(booking);
        }
        catch
        {
            booking.Reject();
            ev.ReleaseSeats();
            
            throw;
        }
        finally
        {
            _processingSemaphor.Release();
        }



    }
}