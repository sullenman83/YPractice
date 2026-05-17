using EventManagement.Common;
using EventManagement.Common.Exceptions;
using EventManagement.Data;
using EventManagement.Interfaces;
using EventManagement.Interfaces.Reposirories;
using EventManagement.Models.BookingModels;
using EventManagement.Models.Events;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.Services;

/// <summary>
/// Фоновый сервис обработки бронирований
/// </summary>
public class BookingHandlerService(ILogger<BackgroundService> logger, IServiceScopeFactory serviceFactory, IDateTimeProvider dateTimeProvider) : BackgroundService
{
    private readonly ILogger<BackgroundService> _logger= logger;
    private readonly IServiceScopeFactory _serviceFactory = serviceFactory;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

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
                var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository<Booking>>();
                var ids = (await bookingRepository.GetPendingBookingsAsync(stoppingToken))
                    .Select(o => o.Id)
                    .ToList();

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
        var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository<Booking>>();
        await using var transaction = await bookingRepository.BeginTransactionAsync(stoppingToken);

        Booking? booking = null;
        try
        {
            booking = await bookingRepository.GetBookingWithBlockingAsync(id, stoppingToken);

            if (booking == null)
                throw new NotFoundException($"Не найдено бронирование с id {id}");

            booking.Confirm(_dateTimeProvider);
            await bookingRepository.SaveChangesAsync(stoppingToken);
            await transaction.CommitAsync(stoppingToken);
            _logger.LogInformation($"Бронирование с id {booking.Id} обработано в {_dateTimeProvider.UtcNow}.");
        }
        catch
        {
            await using var rejectingtSope = _serviceFactory.CreateAsyncScope();
            var rejectingBookingRepository = rejectingtSope.ServiceProvider.GetRequiredService<IBookingRepository<Booking>>();
            await using var rejectingTransaction = await rejectingBookingRepository.BeginTransactionAsync(stoppingToken);
            booking = await bookingRepository.GetBookingWithBlockingAsync(id, stoppingToken);

            if (booking != null)
            {
                booking.Reject(_dateTimeProvider);
                if (!booking.Event?.ReleaseSeats(booking.SeatsCount) ?? false)
                    throw new InvalidOperationException("Количество доступных мест не может быть больше общего количества мест");
                await rejectingBookingRepository.SaveChangesAsync(stoppingToken);
                await rejectingTransaction.CommitAsync(stoppingToken);
            }

            throw;
        }        
    }
}