
using Contracts;
using DateTimeManager.Abstractions;
using Events.Application.Exceptions;
using Events.Application.Interfaces.Repositories;
using Events.Application.Models.Messages;
using Events.Application.Services.MessageHandlers;
using Events.Domain.Exceptions;
using Events.Domain.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TestData;
using TransactionManager.Abstractions;

namespace Events.UnitTests;

public class BookingCancelledHandlerTest
{
    private readonly Mock<IEventRepository> _mockEventRepository = new Mock<IEventRepository>();
    private readonly Mock<IInboxMessageRepository> _mockInboxRepository = new Mock<IInboxMessageRepository>();
    private readonly Mock<ITransactionService> _mockTransactionService = new Mock<ITransactionService>();
    private readonly Mock<ITransaction> _mockTransaction = new Mock<ITransaction>();
    private readonly Mock<ILogger<BookingCancelledHandler>> _mockLogger = new Mock<ILogger<BookingCancelledHandler>>();

    public BookingCancelledHandlerTest()
    {        
        _mockTransactionService.Setup(o => o.BeginTransactionAsync()).ReturnsAsync(_mockTransaction.Object);
    }

    [Fact]
    public async Task HandleMessage_EventSeatsReleased()
    {
        // Arrange
        var totalSeats = 10;
        var seatsCount = 5;
        var ev = EventTestData.GetTestEvent(totalSeats);
        ev.TryReserveSeats(seatsCount);
        _mockEventRepository.Setup(o => o.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(ev);
        var message = new BookingCancelled(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), seatsCount, DateTimeOffset.UtcNow);

        var service = new BookingCancelledHandler(_mockEventRepository.Object, _mockInboxRepository.Object, _mockTransactionService.Object, _mockLogger.Object);

        // Act

        await service.HandleMessageAsync(message, CancellationToken.None);

        // Assert
        _mockEventRepository.Verify(o => o.GetByIdAsync(It.IsAny<Guid>()), Times.Once);
        _mockTransactionService.Verify(o => o.BeginTransactionAsync(), Times.Once);
        _mockEventRepository.Verify(o => o.SaveChangesAsync(), Times.Once);
        _mockInboxRepository.Verify(o => o.AddAsync(It.IsAny<InboxMessage>()), Times.Once);
        _mockTransaction.Verify(o => o.CommitAsync(), Times.Once);
        ev.AvailableSeats.Should().Be(totalSeats);
    }

    [Fact]
    public async Task HandleMessage_IncorrectSeatsCount_ThrowsSeatsCountMoreThenTotalException()
    {
        // Arrange
        var totalSeats = 10;
        var seatsCount = 5;
        var ev = EventTestData.GetTestEvent(totalSeats);
        _mockEventRepository.Setup(o => o.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(ev);
        var message = new BookingCancelled(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), seatsCount, DateTimeOffset.UtcNow);
        
        var service = new BookingCancelledHandler(_mockEventRepository.Object, _mockInboxRepository.Object, _mockTransactionService.Object, _mockLogger.Object);

        // Act

        await service.HandleMessageAsync(message, CancellationToken.None);

        // Assert

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Чесло доступных мест превышает общее количество мест события.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockEventRepository.Verify(o => o.GetByIdAsync(It.IsAny<Guid>()), Times.Once);
        _mockTransactionService.Verify(o => o.BeginTransactionAsync(), Times.Once);
        _mockEventRepository.Verify(o => o.SaveChangesAsync(), Times.Never);
        _mockInboxRepository.Verify(o => o.AddAsync(It.IsAny<InboxMessage>()), Times.Never);
        _mockTransaction.Verify(o => o.CommitAsync(), Times.Never);
        ev.AvailableSeats.Should().Be(totalSeats);
    }

    [Fact]
    public async Task HandleMessage_DublicateInboxMEssage_ThrowsDublicateInsertionException()
    {
        // Arrange
        var totalSeats = 10;
        var seatsCount = 5;
        var ev = EventTestData.GetTestEvent(totalSeats);
        ev.TryReserveSeats(seatsCount);
        _mockEventRepository.Setup(o => o.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(ev);
        var message = new BookingCancelled(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), seatsCount, DateTimeOffset.UtcNow);
        _mockInboxRepository.Setup(o => o.AddAsync(It.IsAny<InboxMessage>())).Throws<DublicateInsertionException>();        
        var service = new BookingCancelledHandler(_mockEventRepository.Object, _mockInboxRepository.Object, _mockTransactionService.Object, _mockLogger.Object);

        // Act

        await service.HandleMessageAsync(message, CancellationToken.None);

        // Assert

        _mockEventRepository.Verify(o => o.GetByIdAsync(It.IsAny<Guid>()), Times.Once);
        _mockTransactionService.Verify(o => o.BeginTransactionAsync(), Times.Once);
        _mockEventRepository.Verify(o => o.SaveChangesAsync(), Times.Once);
        _mockInboxRepository.Verify(o => o.AddAsync(It.IsAny<InboxMessage>()), Times.Once);
        _mockTransaction.Verify(o => o.CommitAsync(), Times.Never);
        ev.AvailableSeats.Should().Be(totalSeats);
    }

    [Fact]
    public async Task HandleMessage_IncorrectEventId_ThrowsNotFoundException()
    {
        // Arrange
        var totalSeats = 10;
        var seatsCount = 5;
        var ev = EventTestData.GetTestEvent(totalSeats);
        ev.TryReserveSeats(seatsCount);
        _mockEventRepository.Setup(o => o.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Guid id, CancellationToken t) => null);
        var message = new BookingCancelled(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), seatsCount, DateTimeOffset.UtcNow);        
        var service = new BookingCancelledHandler(_mockEventRepository.Object, _mockInboxRepository.Object, _mockTransactionService.Object, _mockLogger.Object);

        // Act

        await service.HandleMessageAsync(message, CancellationToken.None);

        // Assert

        _mockEventRepository.Verify(o => o.GetByIdAsync(It.IsAny<Guid>()), Times.Once);
        _mockTransactionService.Verify(o => o.BeginTransactionAsync(), Times.Never);
        _mockEventRepository.Verify(o => o.SaveChangesAsync(), Times.Never);
        _mockInboxRepository.Verify(o => o.AddAsync(It.IsAny<InboxMessage>()), Times.Never);
        _mockTransaction.Verify(o => o.CommitAsync(), Times.Never);
        ev.AvailableSeats.Should().Be(totalSeats - seatsCount);
    }


}
