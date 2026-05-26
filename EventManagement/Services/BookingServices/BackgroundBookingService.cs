using EventManagement.Common.AppSettings;
using EventManagement.Common.Exceptions;
using EventManagement.Interfaces;
using EventManagement.Interfaces.Reposirories;
using EventManagement.Interfaces.Services;
using EventManagement.Models.BookingModels;
using Microsoft.Extensions.Options;

namespace EventManagement.Services.BookingServices;

/// <summary>
/// Сервис обработки событий
/// </summary>
public class BackgroundBookingService(IServiceScopeFactory serviceFactory, ILogger<BackgroundBookingService> logger) : IBackgroundBookingService
{
    private readonly IServiceScopeFactory _serviceFactory = serviceFactory;
    private readonly ILogger<BackgroundBookingService> _logger = logger;

    /// <inheritdoc/>
    public async Task ConfirmBookingAsync(Guid id, CancellationToken token)
    {
        await using var scope = _serviceFactory.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBookingRepository<Booking>>();
        var transactionService = scope.ServiceProvider.GetRequiredService<ITransactionService>();
        var dateTimeProvider = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();

        await using var transaction = await transactionService.BeginTransactionAsync(token);
        
        var booking = await repository.GetBookingWithBlockingAsync(id, token);

        if (booking == null)
            throw new NotFoundException($"Не найдено бронирование с id {id}");
        if (booking.Event == null)
            throw new InvalidOperationException("Непредвиденная ошибка при получении бронирования. Не найдено событие.");

        if (!booking.Event.TryReserveSeats(booking.SeatsCount))
        {
            booking.Reject(dateTimeProvider);
            _logger.LogWarning($"Недостаточно мест для бронирования событие {booking.Event.Id}, бронирование {booking.Id}");
        }
        else
        {
            booking.Confirm(dateTimeProvider);
        }
        await repository.SaveChangesAsync(token);
        await transaction.CommitAsync(token);
    }

    /// <inheritdoc/>
    public async Task RejectBookingAsync(Guid id, CancellationToken token)
    {
        await using var scope = _serviceFactory.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBookingRepository<Booking>>();
        var transactionService = scope.ServiceProvider.GetRequiredService<ITransactionService>();
        var dateTimeProvider = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();        
        await using var tr = await transactionService.BeginTransactionAsync(token);
        
        var booking = await repository.GetBookingWithBlockingAsync(id, token);

        if (booking != null)
        {
            booking.Reject(dateTimeProvider);
           
            await repository.SaveChangesAsync(token);
            await tr.CommitAsync(token);
        }
    }
}
