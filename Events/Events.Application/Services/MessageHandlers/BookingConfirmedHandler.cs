using Contracts;
using DateTimeManager.Abstractions;
using Events.Application.Exceptions;
using Events.Application.Interfaces.MessageHandlers;
using Events.Application.Interfaces.Repositories;
using Events.Application.Interfaces.Validators;
using Events.Application.Models.Messages;
using Events.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using TransactionManager.Abstractions;

namespace Events.Application.Services.MessageHandlers;

/// <summary>
/// Обработчик BookingConfirmen сообщений
/// </summary>
/// <param name="eventRepository">Репозиторий событий</param>
/// <param name="inboxRepository">inbox репозиторий</param>
/// <param name="transactionService">сервис транзакций</param>
/// <param name="dateTimeProvider">Провайдер времени</param>
/// <param name="logger">Логер</param>
/// <param name="validator">Проверка события на валидность даты начала</param>
public class BookingConfirmedHandler(IEventRepository eventRepository, 
    IInboxMessageRepository inboxRepository,
    ITransactionService transactionService,
    IDateTimeProvider dateTimeProvider,
    IBookingConfirmedValidator validator,
    ILogger<BookingConfirmedHandler> logger) 
    : IBookingConfirmedHandler
{
    private readonly IEventRepository _eventRepository = eventRepository;
    private readonly IInboxMessageRepository _inboxMessageRepository = inboxRepository;
    private readonly ITransactionService _transactionService = transactionService;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
    private readonly ILogger<BookingConfirmedHandler> _logger = logger;
    private readonly IBookingConfirmedValidator _validator = validator;

    ///<inheritdoc/>
    public async Task HandleMessageAsync(BookingConfirmed message, CancellationToken token)
    {
        try
        {
            var ev = await _eventRepository.GetByIdAsync(message.EventId, token);
            if (ev == null)
                throw new NotFoundException($"Не найдено событие с id = {message.EventId}");

            _validator.ValidateEventDate(ev.StartAt);

            await using var tr = await _transactionService.BeginTransactionAsync(token);
            if (!ev.TryReserveSeats(message.SeatsCount))
                throw new NoAvailableSeatsException("Недостаточно мест для бронирования.");
            await _eventRepository.SaveChangesAsync();

            await _inboxMessageRepository.AddAsync(new InboxMessage(message.MessageId));            
            await tr.CommitAsync();
        }
        catch(DublicateInsertionException ex)
        {
            _logger.LogInformation(ex.Message);
            return;
        }
        catch(Exception ex)
        {
            _logger.LogError($"Не удалось зарезервировать места для брони '{message.BookingId}' на событие {message.EventId} по причине {ex.Message}");
            
            //ToDo: Тут по хорошему надо было создать outbox сообщение и переправить его в топик проблемных событий (для которых не удалось списать места) и bookings
            // отловить их и отменить бронирование, но на все времени не хватило            
        }
    }
}
