using EventManagement.Application.Common;
using EventManagement.Application.Common.Exceptions;
using EventManagement.Application.Interfaces;
using EventManagement.Application.Interfaces.Repositories;
using EventManagement.Application.Interfaces.Services;
using EventManagement.Application.Interfaces.Services.BookingServices;
using EventManagement.Application.Models.BookingModels;
using EventManagement.Application.Models.BookingModels.Extensions;
using EventManagement.Domain.Exceptions;
using EventManagement.Domain.Models;
using Polly;
using Polly.Registry;

namespace EventManagement.Application.Services.BookingServices;

/// <summary>
/// Сервис для работы с заявками бронирования событий
/// </summary>
public class BookingService(IBookingRepository<Booking> bookingRepository
    , IEventRepository<Event> eventRepoository
    , ITransactionService transactionService
    , IDateTimeProvider dateTimeProvider
    , IBookingValidator bookingValidator
    , ICurrentUserService currentUserService
    , ResiliencePipelineProvider<string> pipelineProvider) :IBookingService
{
    private readonly IBookingRepository<Booking> _bookingRepository = bookingRepository;
    private readonly IEventRepository<Event> _eventRepository = eventRepoository;
    private readonly ITransactionService _transactionService = transactionService;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
    private readonly IBookingValidator _bookingValidator = bookingValidator;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly ResiliencePipeline _resiliencePipeline = pipelineProvider.GetPipeline(Consts.BookingServiceRepeater);

    ///<inheritdoc/>
    /// <exception cref="DbOperationException">Ошибка операций с БД.</exception>
    /// <exception cref="NoAvailableSeatsException">Недостаточно мест для броинрования</exception>
    /// <exception cref="NotFoundException">Не найден объект</exception>        
    /// <exception cref="OperationCanceledException">Операция отменена</exception>
    /// <exception cref="ActiveBookingLimitException">Превышен лимит бронирований</exception>
    /// <exception cref="PastEventBookingException">Событие уже началось</exception>
    public async Task<BookingResponseDTO> CreateBookingAsync(Guid eventId, Guid userId, int seatsCount, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        await ValidateBookingAsync(eventId, userId, token);

        var booking = new Booking(BookingStatus.Pending, eventId, userId, seatsCount, _dateTimeProvider.GetUtcNow());
        return await _resiliencePipeline.ExecuteAsync(async token =>
        {
            await using var tr = await _transactionService.BeginTransactionAsync(token);

            var ev = await _eventRepository.GetEventWithBlockingAsync(eventId, token);
            if (ev == null)
                throw new NotFoundException($"Событие с id {eventId} не найдено в базе данных.");

            if (!ev.TryReserveSeats(seatsCount))
                throw new NoAvailableSeatsException("Нет доступных метс для бронирования");


            await _bookingRepository.AddAsync(booking, token);
            await _eventRepository.SaveChangesAsync(token);
            await tr.CommitAsync();

            return booking.ToResponse();
        });
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

        await _resiliencePipeline.ExecuteAsync(async token =>
        {
            await using var tr = await _transactionService.BeginTransactionAsync(token);
            var booking = await _bookingRepository.GetBookingWithBlockingAsync(id, token);
            if (booking == null)
                throw new NotFoundException($"Бронирование с id {id} не найдено в базе данных.");
            if (booking.Event == null)
                throw new InvalidOperationException("Непредвиденная ошибка при получении бронирования. Не найдено событие.");
            if (booking.User == null)
                throw new InvalidOperationException("Непредвиденная ошибка при получении бронирования. Не найден пользователь.");

            if (booking.Status == BookingStatus.Cancelled
                || booking.Status == BookingStatus.Rejected)
            {
                return;
            }

            if (booking.User.Id != userId && !_currentUserService.IsInRole(UserRole.Admin.ToString()))
                throw new NoRightsException("Недостаточно прав для удаления бронирования");

            booking.Cancel(_dateTimeProvider.GetUtcNow());
            booking.Event.ReleaseSeats(booking.SeatsCount);
            await _bookingRepository.SaveChangesAsync();
            await tr.CommitAsync();
        });
    }
    
    private async Task ValidateBookingAsync(Guid eventId, Guid userId, CancellationToken token)
    {
        var bookings = await _bookingRepository.GetActiveUserBookingAsync(eventId, userId, token);
        var ev = await _eventRepository.GetByIdAsync(eventId);
        if (ev == null)
            throw new NotFoundException($"Не найдено событие с id {eventId}");

        _bookingValidator.ValidateActiveBooking(bookings);
        _bookingValidator.ValidateEventDate(ev.StartAt);
    }
}
