using Contracts;
using Events.Application.Common;
using Events.Application.Exceptions;
using Events.Application.Interfaces;
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
/// <param name="logger">Логер</param>
/// <param name="validator">Проверка события на валидность даты начала</param>
/// <param name="cacheService">Кеш</param>
public class BookingConfirmedHandler(IEventRepository eventRepository, 
    IInboxMessageRepository inboxRepository,
    ITransactionService transactionService,
    IBookingConfirmedValidator validator,
    ILogger<BookingConfirmedHandler> logger,
    ICacheService cacheService) 
    : IBookingConfirmedHandler
{
    private readonly IEventRepository _eventRepository = eventRepository;
    private readonly IInboxMessageRepository _inboxMessageRepository = inboxRepository;
    private readonly ITransactionService _transactionService = transactionService;    
    private readonly ILogger<BookingConfirmedHandler> _logger = logger;
    private readonly IBookingConfirmedValidator _validator = validator;
    private readonly ICacheService _cacheService = cacheService;

    ///<inheritdoc/>
    public async Task HandleMessageAsync(BookingConfirmed message, CancellationToken token)
    {
        try
        {
            var ev = await _eventRepository.GetByIdAsync(message.EventId, token);
            if (ev == null)
            {
                _logger.LogWarning("Не найдено событие с id = {EventId}", message.EventId);
                throw new NotFoundException("Не найдено событие");
            }

            _validator.ValidateEventDate(ev.StartAt);

            await using var tr = await _transactionService.BeginTransactionAsync(token);
            if (!ev.TryReserveSeats(message.SeatsCount))
            {
                _logger.LogWarning("Недостаточно мест для бронирования события {EventId}.", message.EventId);
                throw new NoAvailableSeatsException("Недостаточно мест для бронирования.");
            }
            await _eventRepository.SaveChangesAsync();

            await _inboxMessageRepository.AddAsync(new InboxMessage(message.MessageId));            
            await tr.CommitAsync();

            await _cacheService.DeleteAsync(CacheKeys.EventKey(ev.Id));
        }
        catch(DublicateInsertionException ex)
        {
            _logger.LogInformation(ex.Message);
            return;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Не удалось зарезервировать места для брони {BookingId} на событие {EventId}.", message.BookingId, message.EventId);
            
            //ToDo: Тут по хорошему надо было создать outbox сообщение и переправить его в топик проблемных событий (для которых не удалось списать места) и bookings
            // отловить их и отменить бронирование, но на все времени не хватило            
        }
    }
}
