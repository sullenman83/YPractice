using System.Diagnostics.CodeAnalysis;

namespace EventManagement.Domain.Models;

/// <summary>
/// Класс события
/// </summary>
public class Event
{
    #region Конструкторы
    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="title">Название события </param>
    /// <param name="description">Описание</param>
    /// <param name="startAt">Дата начала</param>
    /// <param name="endAt">Дата завершения</param>
    /// <param name="totalSeats">Общее количество мест для события</param>
    [SetsRequiredMembers]
    public Event(string title, 
        string? description,
        DateTimeOffset startAt,
        DateTimeOffset endAt,
        int totalSeats)
    {
        if (totalSeats <= 0)
            throw new ArgumentException("Количество мест не может быть отрицательным");        
        Id = Guid.NewGuid();
        TotalSeats = totalSeats;
        AvailableSeats = totalSeats;
        Title = title;
        Description = description;
        StartAt = startAt;
        EndAt = endAt;
    }
    private Event() { }
    #endregion

    #region Свойства
    /// <summary>
    /// Идентификатор события
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Название события
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Описание события
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Дата и время начала события. Формат времени dd.MM.yyyy hh:mm:ssZ
    /// </summary>    
    /// <example>22.05.2026 11:34:33+00:00</example>
    public required DateTimeOffset StartAt { get; set; }

    /// <summary>
    /// Дата и время окончания события. Формат времени dd.MM.yyyy hh:mm:ssZ
    /// </summary>
    /// <example>23.05.2026 11:34:33+00:00</example>
    public required DateTimeOffset EndAt { get; set; }

    /// <summary>
    /// ОБщее количество мест
    /// </summary>
    public required int TotalSeats { get; init; }

    /// <summary>
    /// Текущее количество свободных мест
    /// </summary>
    public int AvailableSeats { get; private set; }

    /// <summary>
    /// Коллекция бронирований
    /// </summary>
    public List<Booking>? Bookings { get; set; }
    #endregion

    #region Методы    

    /// <summary>
    /// Зарезервировать количество мест
    /// </summary>
    /// <param name="count">Количество мест</param>
    /// <returns>true - если зарезервировать удалось, false - доступных мест меньше запрашиваемого количества, зарезервировать не удалось</returns>
    public bool TryReserveSeats(int count = 1)
    {
        if (count <= 0)
            throw new ArgumentException("Количество резервируемых мест не может быть отрицательным");
        if (count > AvailableSeats)
            return false;

        AvailableSeats -= count;

        return true;
    }

    /// <summary>
    /// Освободить зарезервированные места
    /// </summary>
    /// <param name="count">Количество мест</param>
    /// <returns>true - если освобождено место, false - не освобождено (количество свободных мест равно максимальному числу мест)</returns>
    public bool ReleaseSeats(int count = 1)
    {
        if (count <= 0)
            throw new ArgumentException("Количество освобождаемых мест не может быть отрицательным");

        if (AvailableSeats + count > TotalSeats)
            return false;

        AvailableSeats += count;

        return true;
    }
    #endregion
}
