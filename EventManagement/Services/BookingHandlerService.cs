using EventManagement.Data;
using EventManagement.Interfaces;
using EventManagement.Models.BookingModels;
using EventManagement.Models.Events;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.Services;

/// <summary>
/// Фоновый сервис обработки бронирований
/// </summary>
public class BookingHandlerService(ILogger<BackgroundService> logger, IDbContextFactory<AppDbContext> dbContextFactory) : BackgroundService
{
    private readonly ILogger<BackgroundService> _logger= logger;
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory = dbContextFactory;

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
                var context = _dbContextFactory.CreateDbContext();
                var tasks = context.Bookings
                    .Where(b => b.Status == BookingStatus.Pending)
                    .Select(o => o.Id)
                    .Select(o => ProcessBookingAsync(o, stoppingToken));

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

    private async Task ProcessBookingAsync(Guid id, CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(ProcessingDelay), stoppingToken);

        var context = _dbContextFactory.CreateDbContext();
        
        Event? ev = null;
        try
        {
            using var transaction = await context.Database.BeginTransactionAsync();

            var booking = context.Bookings.FromSql<Booking>(
@$"SELECT * FROM bookings b 
JOIN events e ON e.id == b.EventId
WHERE b.id == {id}
FOR UPDATE");


            ev = _eventRepository.GetByID(booking.EventId);
            if (ev == null)
            {
                booking.Reject();
                _logger.LogWarning($"Бронирование отклонено. Не найдено событие {booking.EventId} для брони {booking.Id}");
            }
            else
                booking.Confirm();            
            _dbContextFactory.Update(booking);

            _logger.LogInformation($"Бронирование с id {booking.Id} обработано в {DateTimeOffset.UtcNow}.");
        }
        catch(Exception ex)
        {
            booking.Reject();
            if (ev != null)
            {
                if (!ev.ReleaseSeats(booking.SeatsCount))
                    throw new InvalidOperationException("Количество доступных мест не может быть больше общего количества мест");

                _eventRepository.Update(ev);
            }
            _dbContextFactory.Update(booking);
            _logger.LogError(ex, $"Непредвиденная ошибка при обработке бронирования id {booking.Id}");
        }
        finally
        {
            
        }
    }
}