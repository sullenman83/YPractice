using EventManagement.Interfaces;
using EventManagement.Models.BookingModels;

namespace EventManagement.Services;

/// <summary>
/// Фоновый сервис обработки бронирований
/// </summary>
public class BookingHandlerService(ILogger<BackgroundService> logger, IBookingRepository repository ) : BackgroundService
{
    private readonly ILogger<BackgroundService> _logger= logger;
    private readonly IBookingRepository _bookingRepository = repository;

    /// <summary>
    /// Метод сервиса фоновой обработки броней
    /// </summary>
    /// <param name="stoppingToken">Токен отмены</param>
    /// <returns>Пустая задача</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Фогновый сервис обработки бронирований запущен.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var bookings = _bookingRepository.Bookings
                    .Where(o => o.Value.Status == BookingStatus.Pending)
                    .Take(5);

                foreach (var booking in bookings)
                {
                    var isConfirmed = Random.Shared.Next(0, 3) > 0 ? true : false;
                    var b = booking.Value.Clone();
                    b.Status = isConfirmed ? BookingStatus.Confirmed : BookingStatus.Rejected;
                    b.ProcessedAt = DateTime.Now;
                    _bookingRepository.Bookings.TryUpdate(booking.Key, b, booking.Value);

                    _logger.LogInformation($"Бронирование с id {booking.Key} обработано.");

                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
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

        _logger.LogInformation("Фогновый сервис обработки бронирований остановлен.");
    }
}