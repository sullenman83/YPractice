using Bookings.Application.Common;
using Bookings.Application.Exceptions;
using Bookings.Application.Interfaces;
using Bookings.Application.Interfaces.BookingServices;
using Bookings.Application.Interfaces.Repositories;
using Bookings.Application.Models;
using Bookings.Application.Models.Extensions;
using Bookings.Application.Models.Messages;
using Bookings.Domain.Exceptions;
using Bookings.Domain.Models;
using Contracts;
using DateTimeManager.Abstractions;
using System.Text.Json;
using TransactionManager.Abstractions;
using UserRooles;

namespace Bookings.Application.Services.BookingServices;

/// <summary>
/// Сервис для работы с заявками бронирования событий
/// </summary>
public class BookingService(IBookingRepository bookingRepository
    , IDateTimeProvider dateTimeProvider
    , IBookingValidator bookingValidator
    , ICurrentUserService currentUserService
    , ITransactionService transactionService
    , IOutboxMessageRepository outboxRepository
    ): IBookingService

{    
    private readonly IBookingRepository _bookingRepository = bookingRepository;    
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
    private readonly IBookingValidator _bookingValidator = bookingValidator;
    private readonly ICurrentUserService _currentUserService = currentUserService;    
    private readonly ITransactionService _transactionService = transactionService;
    private readonly IOutboxMessageRepository _outboxRepository = outboxRepository;

    ///<inheritdoc/>
    /// <exception cref="DbOperationException">Ошибка операций с БД.</exception>
    /// <exception cref="NotFoundException">Не найден объект</exception>        
    /// <exception cref="OperationCanceledException">Операция отменена</exception>
    /// <exception cref="ActiveBookingLimitException">Превышен лимит бронирований</exception>
    /// <exception cref="PastEventBookingException">Событие уже началось</exception>
    public async Task<BookingResponseDTO> CreateBookingAsync(Guid eventId, Guid userId, int seatsCount, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        await ValidateBookingAsync(eventId, userId, token);

        var booking = new Booking(BookingStatus.Pending, eventId, userId, seatsCount, _dateTimeProvider.GetUtcNow());                        
        await _bookingRepository.AddAsync(booking, token);
        
        return booking.ToResponse();        
    }

    ///<inheritdoc/>
    /// <exception cref="NotFoundException">Не найден объект</exception>
    /// <exception cref="OperationCanceledException">Операция отменена</exception>
    /// <exception cref="DbOperationException">Ошибка операций с БД.</exception>
    public async Task<BookingResponseDTO> GetBookingByIdAsync(Guid bookingId, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var booking = await _bookingRepository.GetByIdAsync(bookingId, token);
        if (booking == null)
            throw new NotFoundException($"Бронирование с id {bookingId} не найдено в базе данных.");
                
        return booking.ToResponse();
    }

    ///<inheritdoc/>
    ///<exception cref="NotFoundException">Не найден объект</exception>
    ///<exception cref="InvalidOperationException">Непредвиденная ошибка</exception>
    ///<exception cref="NoRightsException">Недостаточно прав</exception>
    public async Task CancelBookingAsync(Guid id, Guid userId, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
                
        var booking = await _bookingRepository.GetByIdAsync(id, token);
        if (booking == null)
            throw new NotFoundException($"Бронирование с id {id} не найдено в базе данных.");

        if (booking.Status == BookingStatus.Cancelled
            || booking.Status == BookingStatus.Rejected)
        {
            return;
        }

        if (booking.UserId != userId && !_currentUserService.IsInRole(UserRole.Admin.ToString()))
            throw new NoRightsException("Недостаточно прав для удаления бронирования");

        var message = new BookingCancelled(Guid.NewGuid(), booking.Id, booking.EventId, booking.UserId, booking.SeatsCount, _dateTimeProvider.GetUtcNow());
        var payload = JsonSerializer.Serialize(message);
        var outboxMessage = new OutboxMessage(Guid.NewGuid(), booking.EventId, MessageTypeConsts.BookingCancelled, _dateTimeProvider.GetUtcNow(), payload, 0, false);

        await using var tr = await _transactionService.BeginTransactionAsync(token);
        booking.Cancel(_dateTimeProvider.GetUtcNow());
        await _outboxRepository.AddAsync(outboxMessage, token);
        await _bookingRepository.SaveChangesAsync();
        await tr.CommitAsync();        
    }
    
    private async Task ValidateBookingAsync(Guid eventId, Guid userId, CancellationToken token)
    {
        var bookings = await _bookingRepository.GetActiveUserBookingAsync(userId, token);
                
        _bookingValidator.ValidateActiveBooking(bookings);
        
    }
}
