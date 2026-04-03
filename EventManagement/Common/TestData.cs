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
                Id = new Guid("65F6C3BD-5ADD-4FB0-96C4-2AE9F99F0347"),
                Title = "тестовое событие 1",
                Description = "Описание 1",
                StartAt = DateTime.Parse("2026.03.22"),
                EndAt = DateTime.Parse("2026.03.22"),
            },
            new Event
            {
                Id = new Guid("DF5C3DB1-DA49-4CC2-A646-076F8A6B99C2"),
                Title = "Другое событие для теста 2",
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
    public static List<KeyValuePair<Guid, Event>> GetTestData()
    {
        var events = GetTestEvents();
        var result = new List<KeyValuePair<Guid, Event>>();
        foreach (var e in events)
        {                
            result.Add(new KeyValuePair<Guid, Event>(e.Id, e));
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
