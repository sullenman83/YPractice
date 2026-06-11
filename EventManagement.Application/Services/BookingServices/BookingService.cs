using EventManagement.Application.Common;
using EventManagement.Application.Common.Exceptions;
using EventManagement.Application.Interfaces;
using EventManagement.Application.Interfaces.Repositories;
using EventManagement.Application.Interfaces.Services;
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
    , ResiliencePipelineProvider<string> pipelineProvider) :IBookingService
{
    private readonly IBookingRepository<Booking> _bookingRepository = bookingRepository;
    private readonly IEventRepository<Event> _eventRepository = eventRepoository;
    private readonly ITransactionService _transactionService = transactionService;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
    private readonly ResiliencePipeline _resiliencePipeline = pipelineProvider.GetPipeline(Consts.CreateBookingRepeater);

    /// <summary>
    /// Создать заявку на бронирование события
    /// </summary>
    /// <param name="eventId">Id события </param>
    /// <param name="seatsCount">Количество мест для бронирования</param> 
    /// <param name="token">Токен отмены</param>
    /// <returns>Возвращает объект с описанием брони</returns>
    /// <exception cref="DbOperationException">Ошибка операций с БД.</exception>
    /// <exception cref="NoAvailableSeatsException">Недостаточно мест для броинрования</exception>
    /// <exception cref="NotFoundException">Не найден объект</exception>        
    /// <exception cref="OperationCanceledException">Операция отменена</exception>
    public async Task<BookingResponseDTO> CreateBookingAsync(Guid eventId, int seatsCount, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var booking = new Booking(BookingStatus.Pending, eventId, seatsCount, _dateTimeProvider.GetUtcNow());
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

    /// <summary>
    /// Вернуть бронирование по id
    /// </summary>
    /// <param name="bookingId">Иденификатор брони</param>
    /// <param name="token">Токен отмены</param>    
    /// <returns>Возвращает объект с описанием брони</returns>
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
}
