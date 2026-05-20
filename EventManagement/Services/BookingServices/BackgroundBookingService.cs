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
public class BackgroundBookingService(IServiceScopeFactory serviceFactory) : IBackgroundBookingService
{
    private readonly IServiceScopeFactory _serviceFactory = serviceFactory;

    /// <inheritdoc/>
    public async Task ConfirmBookingAsync(Guid id, CancellationToken token)
    {
        await using var scope = _serviceFactory.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBookingRepository<Booking>>();
        var dateTimeProvider = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();        

        await using var transaction = await repository.BeginTransactionAsync(token);
        
        var booking = await repository.GetBookingWithBlockingAsync(id, token);

        if (booking == null)
            throw new NotFoundException($"Не найдено бронирование с id {id}");

        booking.Process(dateTimeProvider);
        booking.Confirm(dateTimeProvider);
        await repository.SaveChangesAsync(token);
        await transaction.CommitAsync(token);
    }

    /// <inheritdoc/>
    public async Task RejectBookingAsync(Guid id, CancellationToken token)
    {
        await using var scope = _serviceFactory.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBookingRepository<Booking>>();
        var settings = scope.ServiceProvider.GetRequiredService<IOptions<BookingHandlerSettings>>();
        var dateTimeProvider = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();
        await using var tr = await repository.BeginTransactionAsync(token);
        
        var booking = await repository.GetBookingWithBlockingAsync(id, token);

        if (booking != null)
        {
            if (booking.Status == BookingStatus.Pending)
                booking.Process(dateTimeProvider);
            else
            {
                if (booking.ProcessingAt.HasValue
                    && booking.ProcessingAt.Value.AddMilliseconds(settings.Value.MaxProccessingDuration) > dateTimeProvider.UtcNow)
                {
                    booking.Reject(dateTimeProvider);
                    if (!booking.Event?.ReleaseSeats(booking.SeatsCount) ?? false)
                        throw new InvalidOperationException("Количество доступных мест не может быть больше общего количества мест");
                }
                
            }
            await repository.SaveChangesAsync(token);
            await tr.CommitAsync(token);
        }
    }
}
