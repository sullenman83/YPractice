using EventManagement.Models.Events;
using EventManagement.Models.FilterModels;

namespace EventManagement.Interfaces;

/// <summary>
/// Интерфейс сервиса для работы с событиями
/// </summary>
public interface IEventService
{
    /// <summary>
    /// Получить все события
    /// </summary>
    /// <param name="filter">Фильтр событий</param>
    /// <param name="token">Токен отмены операции</param>
    /// <returns>Список событий</returns>
    Task<PaginatedResultDTO> GetEventsAsync(EventFilterRequestDTO filter, CancellationToken token);

    /// <summary>
    /// Получить событие по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор события</param>
    /// <param name="token">Токен отмены операции</param>
    /// <returns>Событие с искомым идентификатором</returns>
    /// <exception cref="ArgumentException">Не найдено событие с заданным id</exception>
    Task<EventResponseDto> GetEventByIdAsync(Guid id, CancellationToken token);

    /// <summary>
    /// Создать событие
    /// </summary>
    /// <param name="event">Данные события</param>
    /// <param name="token">Токен отмены операции</param>
    /// <returns>Созданное событие</returns>
    Task<EventResponseDto> CreateEventAsync(EventRequestDto @event, CancellationToken token);

    /// <summary>
    /// Обновить событие
    /// </summary>
    /// <param name="id">id события</param>
    /// <param name="event">Данные события</param>
    /// <param name="token">Токен отмены операции</param>
    /// <returns>Обновленное событие</returns>
    /// <exception cref="ArgumentException">Не найдено событие с заданным id</exception>
    Task<EventResponseDto> UpdateEventAsync(Guid id,  EventRequestDto @event, CancellationToken token);

    /// <summary>
    /// Удалить событие
    /// </summary>
    /// <param name="id">Идентификатор удаляемого события</param>
    /// <param name="token">Токен отмены операции</param>
    /// <exception cref="ArgumentException">Не найдено событие с заданным id</exception>
    Task DeleteEventAsync(Guid id, CancellationToken token);
}
