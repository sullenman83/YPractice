using Bookings.Application.Common;
using Bookings.Application.Interfaces.BookingServices;
using Bookings.Application.Interfaces.Repositories;
using Bookings.Application.Models.Messages;
using Bookings.Domain.Exceptions;
using Contracts;
using DateTimeManager.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TransactionManager.Abstractions;


namespace Bookings.Application.Services.BookingServices;

/// <summary>
/// Сервис обработки событий
/// </summary>
public class BookingHandlerService(ILogger<BookingHandlerService> logger
    , IOutboxMessageRepository outboxMEssageRepository
    , ITransactionService transactionService
    , IDateTimeProvider dateTimeProvider
    , IBookingRepository bookingRepository
    ) : IBookingHandlerService
{
    private readonly ILogger<BookingHandlerService> _logger = logger;
    private readonly IOutboxMessageRepository _outboxMessageRepository = outboxMEssageRepository;
    private readonly ITransactionService _transactionService = transactionService;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
    private readonly IBookingRepository _bookingRepository = bookingRepository;

    /// <inheritdoc/>
    public async Task ConfirmBookingAsync(Guid id, CancellationToken token)
    {          
        var booking = await _bookingRepository.GetByIdAsync(id, token);
        if (booking == null)
            throw new NotFoundException($"Не найдено бронирование с id {id}");

        await using var transaction = await _transactionService.BeginTransactionAsync(token);
        booking.Confirm(_dateTimeProvider.GetUtcNow());
        var message = new BookingConfirmed(Guid.NewGuid(), booking.Id, booking.EventId, booking.UserId, booking.SeatsCount, _dateTimeProvider.GetUtcNow());
        var payload = JsonSerializer.Serialize(message);
        var outboxMessage = new OutboxMessage(Guid.NewGuid(), booking.EventId, MessageTypeConsts.BookingConfirmed, _dateTimeProvider.GetUtcNow(), payload, 0, false);
        
        await _outboxMessageRepository.AddAsync(outboxMessage, token);
        await _bookingRepository.SaveChangesAsync(token);
        await transaction.CommitAsync();        
    }

    /// <inheritdoc/>
    public async Task RejectBookingAsync(Guid id, CancellationToken token)
    {   
        var booking = await _bookingRepository.GetByIdAsync(id, token);

        if (booking != null)
        {
            booking.Reject(_dateTimeProvider.GetUtcNow());
            await _bookingRepository.SaveChangesAsync(token);
        }
    }
}
