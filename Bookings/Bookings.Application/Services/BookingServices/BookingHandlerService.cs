using Bookings.Application.Interfaces.BookingServices;
using Bookings.Application.Interfaces.Repositories;
using Bookings.Domain.Exceptions;
using DateTimeManager.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TransactionManager.Abstractions;


namespace Bookings.Application.Services.BookingServices;

/// <summary>
/// Сервис обработки событий
/// </summary>
public class BookingHandlerService(IServiceScopeFactory serviceFactory, 
    ILogger<BookingHandlerService> logger
    ) : IBookingHandlerService
{
    private readonly IServiceScopeFactory _serviceFactory = serviceFactory;
    private readonly ILogger<BookingHandlerService> _logger = logger;

    /// <inheritdoc/>
    public async Task ConfirmBookingAsync(Guid id, CancellationToken token)
    {
       
        await using var scope = _serviceFactory.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
        var transactionService = scope.ServiceProvider.GetRequiredService<ITransactionService>();
        var dateTimeProvider = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();

        await using var transaction = await transactionService.BeginTransactionAsync(token);

        var booking = await repository.GetBookingWithBlockingAsync(id, token);

        if (booking == null)
            throw new NotFoundException($"Не найдено бронирование с id {id}");
                        
        booking.Confirm(dateTimeProvider.GetUtcNow());
            
        await repository.SaveChangesAsync(token);
        await transaction.CommitAsync(token);
       
    }

    /// <inheritdoc/>
    public async Task RejectBookingAsync(Guid id, CancellationToken token)
    {
        await using var scope = _serviceFactory.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();       
        var dateTimeProvider = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();        
        
        var booking = await repository.GetByIdAsync(id, token);

        if (booking != null)
        {
            booking.Reject(dateTimeProvider.GetUtcNow());
            await repository.SaveChangesAsync(token);
        }
    }
}
