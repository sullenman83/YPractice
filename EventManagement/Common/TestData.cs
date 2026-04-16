using EventManagement.Models.BookingModels;
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
            new Event(10)
            {                
                Title = "тестовое событие 1",
                Description = "Описание 1",
                StartAt = DateTime.Parse("2026.03.22"),
                EndAt = DateTime.Parse("2026.03.22"),
            },
            new Event(10)
            {             
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


    /// <summary>
    /// Тестовые данные для бронирования событий
    /// </summary>
    /// <returns></returns>
    public static List<Booking> GetTestBookings()
    {
        return new List<Booking>()
        {
            new Booking()
            {
                Id = new Guid("821300A1-4EB9-4006-BF35-A3CC0F756C70"),
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.Parse("2026.03.24 22:00:00"),
                ProcessedAt = DateTime.Parse("2026.03.24 22:00:02"),
                EventId = new Guid("65F6C3BD-5ADD-4FB0-96C4-2AE9F99F0347")
            },

            new Booking()
            {
                Id = new Guid("AF883307-2419-4564-A5C9-88E6DBA40D55"),
                Status = BookingStatus.Rejected,
                CreatedAt = DateTime.Parse("2026.03.25 22:00:00"),
                ProcessedAt = DateTime.Parse("2026.03.25 22:00:02"),
                EventId = new Guid("DF5C3DB1-DA49-4CC2-A646-076F8A6B99C2")
            }
        };
    }

    /// <summary>
    /// Тестовые данные для бронирования событий
    /// </summary>
    /// <returns></returns>
    public static List<KeyValuePair<Guid, Booking>> GetBookingTestData()
    {
        var bookings = GetTestBookings();
        var result = new List<KeyValuePair<Guid, Booking>>();
        foreach (var b in bookings)
        {
            result.Add(new KeyValuePair<Guid, Booking>(b.Id, b));
        }

        return result;
    }

}
