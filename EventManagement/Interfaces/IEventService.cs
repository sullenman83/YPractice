using EventManagement.Models;

namespace EventManagement.Interfaces;

/// <summary>
/// Интерфейс сервиса для работы с событиями
/// </summary>
public interface IEventService
{
    /// <summary>
    /// Получить все события
    /// </summary>
    /// <returns>Список событий</returns>
    List<EventResponseDto> GetAllEvents();

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
    /// <param name="newEvent">Данные события</param>
    void CreateEvent(EventRequestDto @event);

    /// <summary>
    /// Обновить событие
    /// </summary>
    /// <param name="event">Данные события</param>
    /// <exception cref="ArgumentException">Не найдено событие с заданным id</exception>
    void UpdateEvent(int id,  EventRequestDto @event);

    /// <summary>
    /// Удалить событие
    /// </summary>
    /// <param name="id">Идентификатор удаляемого события</param>
    void DeleteEvent(int id);

}
