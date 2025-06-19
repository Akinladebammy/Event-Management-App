using EventManagementApp.Models.Entities;

namespace EventManagementApp.Repositories.Interfaces
{
    public interface IEventRepository
    {
        Task<IEnumerable<Event>> GetAllAsync();
        Task<Event> GetByIdAsync(Guid id);
        Task<Event> AddAsync(Event eventItem);
        Task<Event> UpdateAsync(Event eventItem);
        Task<bool> DeleteAsync(Guid id);
        Task<int> GetBookingCountAsync(Guid eventId);
    }
}
