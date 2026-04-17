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
            new Event("тестовое событие 1",
                "Описание 1",
                DateTime.Parse("2026.03.22"),
                DateTime.Parse("2026.03.22"),
                10)
            ,
            new Event("Другое событие для теста 2",
                "Описание 21",
                DateTime.Parse("2026.03.24"),
                DateTime.Parse("2026.03.27"),
                10)           
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
    public static EventCreationDTO GetTestEvent()
    {
        var startAt = DateTime.Parse("2026.03.22");
        var endAt = DateTime.Parse("2026.03.24");
        return new EventCreationDTO()
        {
            Title = "Тестовое событие",
            Description = "Описание тестового события",
            StartAt = startAt,
            EndAt = endAt,
            TotalSeats = 10            
        };
    }


    /// <summary>
    /// Тестовые данные для бронирования событий
    /// </summary>
    /// <returns></returns>
    public static List<Booking> GetTestBookings()
    {
        var events = GetTestEvents();

        return new List<Booking>()
        {
            new Booking(BookingStatus.Pending, events[0].Id)
            {
                CreatedAt = DateTime.Parse("2026.03.24 22:00:00"),
                ProcessedAt = DateTime.Parse("2026.03.24 22:00:02"),              
            },

            new Booking(BookingStatus.Rejected, events[1].Id)
            {
                CreatedAt = DateTime.Parse("2026.03.25 22:00:00"),
                ProcessedAt = DateTime.Parse("2026.03.25 22:00:02"),
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
