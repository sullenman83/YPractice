using EventManagement.Models.Events;

namespace EventManagement.Common;


/// <summary>
/// Генератор тестовых данных
/// </summary>
public static class TestData
{
    /// <summary>
    /// Сгенерировать коллекцию событий
    /// </summary>
    /// <returns>Коллекция событий</returns>
    public static List<Event> GetTestEvents()
    {
        return new List<Event>()
        {
            new Event
            {
                Id = 1,
                Title = "Событие 1",
                Description = "Описание 1",
                StartAt = DateTime.Parse("2026.03.22"),
                EndAt = DateTime.Parse("2026.03.22"),
            },
            new Event
            {
                Id = 2,
                Title = "Событие 2",
                Description = "Описание 21",
                StartAt = DateTime.Parse("2026.03.24"),
                EndAt = DateTime.Parse("2026.03.27")
            }
        };
    }

    /// <summary>
    /// Сгенерировать коллекцию пар ключ значение 
    /// </summary>
    /// <returns>Коллекция пар ключ значнеие</returns>
    public static List<KeyValuePair<int, Event>> GetTestData()
    {
        var events = GetTestEvents();
        var result = new List<KeyValuePair<int, Event>>();
        foreach (var e in events)
        {                
            result.Add(new KeyValuePair<int, Event>(e.Id, e));
        }

        return result;
    }

    /// <summary>
    /// Сгенерировать одиночное событие
    /// </summary>
    /// <returns>Событие</returns>
    public static EventRequestDto GetTestEvent()
    {
        var startAt = DateTime.Parse("2026.03.22");
        var endAt = DateTime.Parse("2026.03.24");
        return new EventRequestDto()
        {
            Title = "Тестовое событие",
            Description = "Описание тестового события",
            StartAt = startAt,
            EndAt = endAt,
        };
    }
}
