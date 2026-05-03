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
    public static List<Event> GetTestEvents(int seats = 10)
    {
        return new List<Event>()
        {
            new Event("тестовое событие 1",
                "Описание 1",
                DateTimeOffset.Parse("2026.03.22 18:30:00 +0:00"),
                DateTimeOffset.Parse("2026.03.22 18:30:00 +0:00"),
                seats)
            ,
            new Event("Другое событие для теста 2",
                "Описание 21",
                DateTimeOffset.Parse("2026.03.24 18:30:00 +0:00"),
                DateTimeOffset.Parse("2026.03.27 18:30:00 +0:00"),
                seats)           
        };
    }

    /// <summary>
    /// Сгенерировать одиночное событие
    /// </summary>
    /// <returns>Событие</returns>
    public static EventCreationDTO GetTestEventCreationDTO()
    {
        var ev = GetTestEvent();
        return new EventCreationDTO()
        {
            Title = ev.Title,
            Description = ev.Description,
            StartAt = ev.StartAt,
            EndAt = ev.EndAt,
            TotalSeats = ev.TotalSeats,            
        };
    }

    /// <summary>
    /// Создать тестовое событие
    /// </summary>
    /// <returns>Событие</returns>
    public static Event GetTestEvent(int seats = 10)
    {
        var startAt = DateTimeOffset.Parse("2026.03.22 18:30:00 +0:00");
        var endAt = DateTimeOffset.Parse("2026.03.24 18:30:00 +0:00");
        return new Event(        
            "Тестовое событие",
            "Описание тестового события",
            startAt,
            endAt,
            seats
        );
    }

    /// <summary>
    /// Сконвертироваться в DTO сущность
    /// </summary>
    /// <param name="ev">Событие</param>
    /// <returns>DTO сущность</returns>
    public static EventCreationDTO ToCreationDTO(this Event ev)
    {
        return new EventCreationDTO()
        {
            Title = ev.Title,
            Description = ev.Description,
            StartAt = ev.StartAt,
            EndAt = ev.EndAt,
            TotalSeats = ev.TotalSeats,
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
            new Booking(BookingStatus.Pending, events[0].Id, 1, DateTimeOffset.Parse("2026.03.24 18:30:00 +0:00"))
            {                
                ProcessedAt = DateTimeOffset.Parse("2026.03.24 18:30:02 +0:00"),
            },

            new Booking(BookingStatus.Rejected, events[1].Id, 1, DateTimeOffset.Parse("2026.03.25 18:30:00 +0:00"))
            {
                ProcessedAt = DateTimeOffset.Parse("2026.03.25 18:30:02 +0:00"),
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
