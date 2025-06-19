using EventManagementApp.DTOs;
using EventManagementApp.Models.Entities;

namespace EventManagementApp.Services.Interfaces
{
    public interface IEventService
    {
        Task<IEnumerable<EventDto>> GetAllEventsAsync(Guid? currentUserId = null);
        Task<EventDto> GetEventByIdAsync(Guid id, Guid? currentUserId = null);
        Task<Event> CreateEventAsync(Event eventItem);
        Task<Event> UpdateEventAsync(Event eventItem);
        Task<bool> DeleteEventAsync(Guid id);
        Task<int> GetAvailableSpotsAsync(Guid eventId);
    }
}