using EventManagementApp.Data;
using EventManagementApp.Models.Entities;
using EventManagementApp.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventManagementApp.Repositories.Implementations
{
    public class EventRepository : IEventRepository
    {
        private readonly ApplicationDbContext _context;

        public EventRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Event>> GetAllAsync()
        {
            return await _context.Events.OrderBy(e => e.Date).ToListAsync();
        }

        public async Task<Event> GetByIdAsync(Guid id)
        {
            return await _context.Events.FindAsync(id);
        }

        public async Task<Event> AddAsync(Event eventItem)
        {
            eventItem.Id = Guid.NewGuid();
            _context.Events.Add(eventItem);
            await _context.SaveChangesAsync();
            return eventItem;
        }

        public async Task<Event> UpdateAsync(Event eventItem)
        {
            _context.Events.Update(eventItem);
            await _context.SaveChangesAsync();
            return eventItem;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem == null) return false;

            _context.Events.Remove(eventItem);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetBookingCountAsync(Guid eventId)
        {
            return await _context.EventBookings.CountAsync(eb => eb.EventId == eventId);
        }
    }
}
