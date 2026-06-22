using EventManagement.Application.Models.Events;
using EventManagement.Application.Models.UserModels;
using EventManagement.Domain.Models;

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
                DateTimeOffset.Parse("2026.03.23 18:30:00 +0:00"),
                seats)
            ,
            new Event("Другое событие для теста 2",
                "Описание 2",
                DateTimeOffset.Parse("2026.03.26 18:30:00 +0:00"),
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
        return  GetTestEvent().ToCreationDTO();
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
        var user = GetTestUser();

        return new List<Booking>()
        {
            new Booking(BookingStatus.Pending, events[0].Id, user.Id, 1, DateTimeOffset.Parse("2026.03.24 18:30:00 +0:00"))
            {                
                ProcessedAt = DateTimeOffset.Parse("2026.03.24 18:30:02 +0:00"),
            },

            new Booking(BookingStatus.Rejected, events[1].Id, user.Id, 1, DateTimeOffset.Parse("2026.03.25 18:30:00 +0:00"))
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

    /// <summary>
    /// Сгенерировать бронирование
    /// </summary>
    /// <param name="ev">Событие</param>
    /// <param name="dateTime">Время создания брони</param>
    /// <param name="seatsCount">Сколько мест бронируется</param>
    /// <param name="status">Статус брони</param>
    /// <returns>Бронирование</returns>
    public static Booking GetTestBooking(Event ev, User user, DateTimeOffset dateTime, int seatsCount = 1, BookingStatus status = BookingStatus.Pending)
    {        
        return new Booking(status, ev, user, seatsCount, dateTime);
    }

    /// <summary>
    /// Получить тестового пользователя
    /// </summary>
    /// <returns>Тестовый пользователь</returns>
    public static User GetTestUser(UserRole role = UserRole.User)
    {
        return new User("user", "password", UserRole.User);        
    }

    /// <summary>
    /// Получить пользователя для мередачи в метод создания пользователя
    /// </summary>
    /// <returns>DTO польщователь</returns>
    public static UserRequestDTO getRequestUser()
    {
        return new UserRequestDTO()
        {
            Login = "user",
            Password = "password",
            Role = UserRole.User
        };
    }    
}
