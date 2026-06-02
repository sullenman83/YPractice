using EventManagement.Application.Common;
using EventManagement.Application.Common.Exceptions;
using EventManagement.Application.Interfaces.Reposirories;
using EventManagement.Application.Interfaces.Services;
using EventManagement.Domain.Interfaces;
using EventManagement.Domain.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Registry;

namespace EventManagement.Application.Services.BookingServices;

/// <summary>
/// Сервис обработки событий
/// </summary>
public class BackgroundBookingService(IServiceScopeFactory serviceFactory, 
    ILogger<BackgroundBookingService> logger,
    ResiliencePipelineProvider<string> pipelineProvider
    ) : IBackgroundBookingService
{
    private readonly IServiceScopeFactory _serviceFactory = serviceFactory;
    private readonly ILogger<BackgroundBookingService> _logger = logger;
    private readonly ResiliencePipeline _pipeline = pipelineProvider.GetPipeline(Consts.CreateBookingRetry);

    /// <inheritdoc/>
    public async Task ConfirmBookingAsync(Guid id, CancellationToken token)
    {
        await _pipeline.ExecuteAsync(async token =>
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
        });
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
