using Contracts;
using Events.Application.Interfaces.MessageHandlers;
using Events.Application.Interfaces.Repositories;

namespace Events.Application.Services.MessageHandlers;

public class BokingConfirmedHandler(IEventRepository eventRepository, IInbox) : IBookingConfirmedHandler
{
    public Task HandleMessage(BookingConfirmed message)
    {
        
    }
}
