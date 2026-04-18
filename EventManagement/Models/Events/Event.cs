using System.Diagnostics.CodeAnalysis;

namespace EventManagement.Models.Events;

/// <summary>
/// Класс события
/// </summary>
public class Event
{
    private Guid _id = Guid.NewGuid();
    private int _totalSeats;
    private int _availableSeats;

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
        DateTime startAt,
        DateTime endAt,
        int totalSeats)
    {
        _totalSeats = totalSeats;
        _availableSeats = totalSeats;
        Title = title;
        Description = description;
        StartAt = startAt;
        EndAt = endAt;
    }
    private Event() { }

    /// <summary>
    /// Идентификатор события
    /// </summary>
    public Guid Id => _id;

    /// <summary>
    /// Название события
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Описание события
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Дата и время начала события
    /// </summary>
    public required DateTime StartAt { get; set; }

    /// <summary>
    /// Дата и время окончания события
    /// </summary>
    public required DateTime EndAt { get; set; }

    /// <summary>
    /// ОБщее количество мест
    /// </summary>
    public int TotalSeats => _totalSeats;

    /// <summary>
    /// Текущее количество свободных мест
    /// </summary>
    public int AvailableSeats => _availableSeats;
    /// <summary>
    /// Создать корпию события
    /// </summary>
    /// <returns>Копия события</returns>
    public Event Clone()
    {
        return new Event()
        {
            _id = Id,
            Title = Title,
            Description = Description,
            StartAt = StartAt,
            EndAt = EndAt,
            _totalSeats = TotalSeats,
            _availableSeats = AvailableSeats
        };
    }

    /// <summary>
    /// Зарезервировать количество мест
    /// </summary>
    /// <param name="count">Количество мест</param>
    /// <returns>true - если зарезервировать удалось, false - доступных мест меньше запрашиваемого количества, зарезервировать не удалось</returns>
    public bool TryReserveSeats(int count = 1)
    {
        if (count > AvailableSeats)
            return false;

        _availableSeats -= count;

        return true;
    }

    /// <summary>
    /// Освободить зарезервированные места
    /// </summary>
    /// <param name="count">Количество мест</param>
    /// <returns>true - если освобождено место, false - не освобождено (количество свободных мест равно максимальному числу мест)</returns>
    public bool ReleaseSeats(int count = 1)
    {
        if (_availableSeats == _totalSeats)
            return false;

        _availableSeats += count;
        return true;
    }
}
