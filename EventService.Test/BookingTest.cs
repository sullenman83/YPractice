using EventManagement.Common;
using EventManagement.Common.Exceptions;
using EventManagement.Data;
using EventManagement.Interfaces;
using EventManagement.Models.BookingModels;
using EventManagement.Models.Events;
using EventManagement.Models.FilterModels;
using EventManagement.Services.BookingServices;
using EventManagement.Services.EventServices;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EventServiceTest;

public class BookingTest
{
    private readonly IEventValidator _eventValidator;
    private readonly ServiceProvider _serviceProvider;
    private readonly IEventService _eventService;
    private readonly IServiceScope _scope;
    private readonly IBookingService _bookingService;

    public BookingTest()
    {
        var services = new ServiceCollection();
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IEventValidator, EventValidator>();
        services.AddScoped<IBookingService, BookingService>();

        var dbName = Guid.NewGuid().ToString();
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseInMemoryDatabase(dbName)
            .ConfigureWarnings(cfg =>
            {
                cfg.Ignore(InMemoryEventId.TransactionIgnoredWarning);
            });
        });

        _serviceProvider = services.BuildServiceProvider();
        _scope = _serviceProvider.CreateScope();
        _eventService = _scope.ServiceProvider.GetRequiredService<IEventService>();
        _eventValidator = _scope.ServiceProvider.GetRequiredService<IEventValidator>();
        _bookingService = _scope.ServiceProvider.GetRequiredService<IBookingService>();
    }

    [Fact]
    public async Task CreateBooking_ByEventId_ReturnBookingWithPendingStatus()
    {
        // Arrange
        var ev = await CreateTestEvent();
        var id = ev.Id;
        var seats = 5;
        
        // Act
        var result = await _bookingService.CreateBookingAsync(id, seats, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.EventId.Should().Be(id);
        result.Status.Should().Be(BookingStatus.Pending);
        ev.AvailableSeats.Should().Be(ev.TotalSeats - seats);
    }

    [Fact]
    public async Task CreateSeveralBookings_ByEventID_ReturnsUniqueBookingId()
    {
        // Arrange
        var events = await CreateTestEvent();
        var id = events.Id;
        var ids = new List<Guid>();
        
        // Act
        for (int i = 0; i < 3; ++i)
        {
            var result = await _bookingService.CreateBookingAsync(id, 1, CancellationToken.None);
            ids.Add(result.Id);
        }

        // Assert
        ids.Should().HaveCount(3);
        ids.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task GetBooking_ById_ReturnBookig()
    {
        // Arrange
                
        var ev = await CreateTestEvent();
        var booking = await _bookingService.CreateBookingAsync(ev.Id, 1, CancellationToken.None);
                
        // Act
        var result = await _bookingService.GetBookingByIdAsync(booking.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.EventId.Should().Be(booking.EventId);
        result.Id.Should().Be(booking.Id);
        result.Status.Should().Be(BookingStatus.Pending);
    }

    [Fact]
    public async Task GetBooking_ChangeStatus_ReturnChangedStatus()
    {
        // Arrange        
        var eventID = Guid.NewGuid();
        var booking = new Booking(BookingStatus.Pending, eventID, 1, DateTimeOffset.UtcNow);               

        // Act        
        booking.Reject();

        // Assert
        booking.Status.Should().Be(BookingStatus.Rejected);
    }

    [Fact]
    public async Task GetBooking_ByInvalidEventId_SholdThrowsNotFoundException()
    {
        // Arrange        
        var eventId = Guid.NewGuid();

        // Act
        Func<Task> act = async () => await _bookingService.CreateBookingAsync(eventId, 2, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetBooking_ByDeletedEventId_SholdThrowsNotFoundException()
    {
        // Arrange
        var ev = TestData.GetTestEventCreationDTO();
        var newEvent = await _eventService.CreateEventAsync(ev, CancellationToken.None);        
        var eventId = newEvent.Id;
        var setasCnt = 2;
        var booking = await _bookingService.CreateBookingAsync(eventId, setasCnt, CancellationToken.None);

        // Act
        await _eventService.DeleteEventAsync(eventId, CancellationToken.None);
        Func<Task> act = async () => await _bookingService.CreateBookingAsync(eventId, setasCnt, CancellationToken.None);

        // Assert
        booking.Should().NotBeNull();
        booking.EventId.Should().Be(eventId);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetBooking_ByInvalidBookingId_ShouldThrowsNotFoundException()
    {
        // Arrange        
        var bookingId = Guid.NewGuid();
        
        // Act
        Func<Task> act = async () => await _bookingService.GetBookingByIdAsync(bookingId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }


    [Fact]
    public async Task CreateBooking_OneSeat_ReturnReducedSeatsNumber()
    {
        // Arrange
        var ev = await CreateTestEvent();
        var availableSeats = ev.AvailableSeats;
        var id = ev.Id;
        var seatsCnt = 1;

        // Act
        var result = await _bookingService.CreateBookingAsync(id, seatsCnt, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.EventId.Should().Be(id);
        ev.AvailableSeats.Should().Be(availableSeats - seatsCnt);
    }

    [Fact]
    public async Task CreateSeveralBooking_ReturnBookingsWhitUniqueId()
    {
        // Arrange
        var ev = await CreateTestEvent();
        var id = ev.Id;
        var seats = 1;
        var ids = new List<Guid>();
        var cnt = 3;
        
        // Act
        for (int i = 0; i < cnt; ++i)
        {
            var result = await _bookingService.CreateBookingAsync(id, seats, CancellationToken.None);
            ids.Add(result.Id);
        }

        // Assert
        ev.AvailableSeats.Should().BeGreaterThan(seats * cnt);
        ids.Should().HaveCount(cnt);
        ids.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task CreateSeveralBooking_ExecuteCountMoreThenTotalSeats_ThrowsNoAvailableSeatsException()
    {
        // Arrange        
        var totalSeats = 4;
        var ev = TestData.GetTestEvent(totalSeats);

        var request = ev.ToCreationDTO();
        var newEvent = await _eventService.CreateEventAsync(request, CancellationToken.None);
        
        var id = newEvent.Id;
        var seats = 1;
        var ids = new List<Guid>();
        var cnt = 3;

        // Act
        for (int i = 0; i < cnt; ++i)
        {
            var result = await _bookingService.CreateBookingAsync(id, seats, CancellationToken.None);
            ids.Add(result.Id);
        }
        Func<Task<BookingResponseDTO>> act = async () => await _bookingService.CreateBookingAsync(id, seats, CancellationToken.None);

        // Assert
        ids.Should().HaveCount(cnt);
        ids.Should().OnlyHaveUniqueItems();
        await act.Should().ThrowAsync<NoAvailableSeatsException>();
    }

    [Fact]
    public async Task CreateBooking_NoAvailableSeats_ThrowsNoAvailableSeatsException()
    {
        //Arrange
        var totalSeats = 1;
        var ev = TestData.GetTestEvent(totalSeats);

        var request = ev.ToCreationDTO();
        var newEvent = await _eventService.CreateEventAsync(request, CancellationToken.None);
        var id = ev.Id;
        var seatsCount = 2;                      
        
        //Act
        Func<Task<BookingResponseDTO>> act = async () => await _bookingService.CreateBookingAsync(id, seatsCount, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NoAvailableSeatsException>();
    }

    [Fact]
    public async Task CreateBooking_SetConfirm_ReturnChangedBooking()
    {
        // Arrange                
        var eventID = Guid.NewGuid();
        var booking = new Booking(BookingStatus.Pending, eventID, 1, DateTimeOffset.UtcNow);

        // Act        
        booking.Confirm();

        // Assert
        booking.Status.Should().Be(BookingStatus.Confirmed);
    }    

    [Fact]
    public async Task ReleaseSeatsAfteReject_ReturnRightAvailableSeats()
    {
        // Arrange        
        int cnt = 5;
        var ev = TestData.GetTestEvent(10);
        var booking = new Booking(BookingStatus.Confirmed, Guid.NewGuid(), cnt, DateTimeOffset.UtcNow);
        ev.TryReserveSeats(cnt);
        var availableSeats = ev.AvailableSeats;

        // Act        
        booking.Reject();
        ev.ReleaseSeats(cnt);

        // Assert
        booking.Status.Should().Be(BookingStatus.Rejected);
        availableSeats.Should().Be(ev.TotalSeats - cnt);
        ev.AvailableSeats.Should().Be(ev.TotalSeats);
    }

    [Fact]
    public async Task BookingSeatsAfterRelease()
    {
        // Arrange        
        int cnt = 5;
        var ev = TestData.GetTestEvent(10);
        var booking = new Booking(BookingStatus.Confirmed, Guid.NewGuid(), cnt, DateTimeOffset.UtcNow);
        ev.TryReserveSeats(cnt);
        var availableSeats = ev.AvailableSeats;

        // Act        
        booking.Reject();
        ev.ReleaseSeats(cnt);
        var availableSeats1 = ev.AvailableSeats;
        ev.TryReserveSeats(cnt);

        // Assert
        booking.Status.Should().Be(BookingStatus.Rejected);
        availableSeats.Should().Be(ev.TotalSeats - cnt);
        availableSeats1.Should().Be(ev.TotalSeats);
        ev.AvailableSeats.Should().Be(ev.TotalSeats - cnt);
    }

    [Fact]
    public async Task OverbookingProtectionTest_ReturnRightSuccessBooking()
    {
        // Arrange
        int totalSeats = 5;
        int requestCnt = 20;
        var ev = await CreateTestEvent(totalSeats);
        var tasks = new List<Task<BookingResponseDTO>>();
        var bookingCnt = 1;

        var noAvailableSeatsExceptionCount = 0;

        for (int i = 0; i < requestCnt; ++i)
        {
            tasks.Add(Task.Run(async () => await _bookingService.CreateBookingAsync(ev.Id, bookingCnt, CancellationToken.None)));
        }
        var task = Task.WhenAll(tasks);

        // Act
        try
        {
            await task;
        }
        catch (Exception)
        {
            noAvailableSeatsExceptionCount = task.Exception?.InnerExceptions.OfType<NoAvailableSeatsException>().Count()
                ?? throw new Exception("Что-то работает не так");
        }

        var success = tasks.Where(t => t.Status == TaskStatus.RanToCompletion).Count();
        var failed = tasks.Where(t => t.Status == TaskStatus.Faulted).Count();

        // Assert
        success.Should().Be(totalSeats);
        failed.Should().Be(requestCnt - totalSeats);
        ev.AvailableSeats.Should().Be(0);
        noAvailableSeatsExceptionCount.Should().Be(requestCnt - totalSeats);
    }


    [Fact]
    public async Task ConcurentBooking_ReturnUniqueId()
    {
        // Arrange
        int totalSeats = 10;
        int requestCnt = 10;
        var ev = TestData.GetTestEvent(totalSeats);
        var tasks = new List<Task<BookingResponseDTO>>();
        var bookingCnt = 1;

        var creatonEvent = new EventCreationDTO()
        {
            Title = ev.Title,
            Description = ev.Description,
            TotalSeats = ev.TotalSeats,
            StartAt = ev.StartAt,
            EndAt = ev.EndAt
        };

        var newEvent = await _eventService.CreateEventAsync(creatonEvent, CancellationToken.None);
        
        for (int i = 0; i < requestCnt; ++i)
        {
            tasks.Add(Task.Run(async () => await _bookingService.CreateBookingAsync(newEvent.Id, bookingCnt, CancellationToken.None)));
        }
        var task = Task.WhenAll(tasks);

        // Act        
        var result = await task;

        // Assert
        result.Should().OnlyHaveUniqueItems(o => o.Id);
    }

    [Fact]
    public async Task CreateBooking_NegativeSeatsCount_ThrowsArgumentException()
    {
        // Arrange                
        var ev = await CreateTestEvent();
        var id = ev.Id;
        var bookingCnt = -1;       

        // Act
        Func<Task<BookingResponseDTO>> act = async () => await _bookingService.CreateBookingAsync(id, bookingCnt, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ReleaseSeats_MoreThenTotalSeats_ThrowsInvalidOperationException()
    {
        // Arrange
        int totalSeats = 10;
        var ev = TestData.GetTestEvent(totalSeats);
        var seatsCount = 1;

        // Act
        ev.TryReserveSeats(seatsCount);
        var result1 = ev.ReleaseSeats(seatsCount);
        var result2 = ev.ReleaseSeats(seatsCount);

        // Assert
        result1.Should().BeTrue();
        result2.Should().BeFalse();
    }


    private async Task<EventResponseDto> CreateTestEvent(int seatsCnt = 10)
    {
        var ev = TestData.GetTestEvent(seatsCnt);
        return  await _eventService.CreateEventAsync(ev.ToCreationDTO(), CancellationToken.None);
    }
}