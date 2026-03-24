using EventManagement.Models;
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
    /// <returns>Список событий</returns>
    PaginatedResultDTO GetEvents(EventFilterRequestDTO filter);

    /// <summary>
    /// Получить событие по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор события</param>
    /// <returns>Событие с искомым идентификатором</returns>
    /// <exception cref="ArgumentException">Не найдено событие с заданным id</exception>
    EventResponseDto GetEventById(int id);

    /// <summary>
    /// Создать событие
    /// </summary>
    /// <param name="event">Данные события</param>
    /// <returns>Созданное событие</returns>
    EventResponseDto CreateEvent(EventRequestDto @event);

    /// <summary>
    /// Обновить событие
    /// </summary>
    /// <param name="id">id события</param>
    /// <param name="event">Данные события</param>
    /// <returns>Обновленное событие</returns>
    /// <exception cref="ArgumentException">Не найдено событие с заданным id</exception>
    EventResponseDto UpdateEvent(int id,  EventRequestDto @event);

    /// <summary>
    /// Удалить событие
    /// </summary>
    /// <param name="id">Идентификатор удаляемого события</param>
    /// <exception cref="ArgumentException">Не найдено событие с заданным id</exception>
    void DeleteEvent(int id);
}
