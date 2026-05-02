using EventManagement.Data;
using EventManagement.Interfaces;
using EventManagement.Models.BookingModels;
using EventManagement.Models.Events;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.Services;

/// <summary>
/// Фоновый сервис обработки бронирований
/// </summary>
public class BookingHandlerService(ILogger<BackgroundService> logger, IServiceScopeFactory serviceFactory) : BackgroundService
{
    private readonly ILogger<BackgroundService> _logger= logger;
    private readonly IServiceScopeFactory _serviceFactory = serviceFactory;

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
                await using var scope = _serviceFactory.CreateAsyncScope();
                List<Guid> ids;
                using (var context = scope.ServiceProvider.GetRequiredService<AppDbContext>())
                {
                    ids = await context.Bookings
                        .Where(b => b.Status == BookingStatus.Pending)
                        .Select(o => o.Id)
                        .ToListAsync();
                }                    
               var tasks = ids.Select(o => ProcessBookingAsync(o, stoppingToken));

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

        await using var scope = _serviceFactory.CreateAsyncScope();
        using var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Booking? booking = null;
        try
        {
            using var transaction = await context.Database.BeginTransactionAsync();

            booking = await context.Bookings.FromSql(
@$"SELECT b.*    
FROM bookings b 
JOIN events e ON e.id = b.event_id
WHERE b.id = {id}
FOR UPDATE")
                .Include(o => o.Event)
                .FirstAsync();

            if (booking == null)
                throw new DirectoryNotFoundException($"Не найдено бронирование с id {id}");

            booking.Confirm();
            //var ev = await context.Events.FirstOrDefaultAsync(o => o.Id == booking.EventId);
            //if (ev == null)
            //{
            //    booking.Reject();
            //    _logger.LogWarning($"Бронирование отклонено. Не найдено событие {booking.EventId} для брони {booking.Id}");
            //}
            //else
            //    booking.Confirm();

            _logger.LogInformation($"Бронирование с id {booking.Id} обработано в {DateTimeOffset.UtcNow}.");
        }
        catch(Exception ex)
        {
            if (booking != null)
            {
                booking.Reject();
                if (!booking.Event?.ReleaseSeats(booking.SeatsCount) ?? false)
                    throw new InvalidOperationException("Количество доступных мест не может быть больше общего количества мест");
                await context.SaveChangesAsync();
            }
            
            _logger.LogError(ex, $"Непредвиденная ошибка при обработке бронирования id {booking?.Id}");
        }        
    }
}