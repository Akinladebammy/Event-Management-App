using EventManagementApp.DTOs;
using EventManagementApp.Models.Entities;
using EventManagementApp.Repositories.Interfaces;
using EventManagementApp.Services.Interfaces;

namespace EventManagementApp.Services.Implementations
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;
        private readonly IBookingRepository _bookingRepository;

        public EventService(IEventRepository eventRepository, IBookingRepository bookingRepository)
        {
            _eventRepository = eventRepository;
            _bookingRepository = bookingRepository;
        }

        public async Task<IEnumerable<EventDto>> GetAllEventsAsync(Guid? currentUserId = null)
        {
            var events = await _eventRepository.GetAllAsync();
            var eventDtos = new List<EventDto>();

            foreach (var eventItem in events)
            {
                var availableSpots = await GetAvailableSpotsAsync(eventItem.Id);
                var isBookedByCurrentUser = currentUserId.HasValue
                    ? await _bookingRepository.ExistsAsync(eventItem.Id, currentUserId.Value)
                    : false;

                eventDtos.Add(new EventDto
                {
                    Id = eventItem.Id,
                    Title = eventItem.Title,
                    Description = eventItem.Description,
                    Date = eventItem.Date,
                    Location = eventItem.Location,
                    Capacity = eventItem.Capacity,
                    AvailableSpots = availableSpots,
                    IsBookedByCurrentUser = isBookedByCurrentUser
                });
            }

            return eventDtos;
        }

        public async Task<EventDto> GetEventByIdAsync(Guid id, Guid? currentUserId = null)
        {
            var eventItem = await _eventRepository.GetByIdAsync(id);
            if (eventItem == null) return null;

            var availableSpots = await GetAvailableSpotsAsync(eventItem.Id);
            var isBookedByCurrentUser = currentUserId.HasValue
                ? await _bookingRepository.ExistsAsync(eventItem.Id, currentUserId.Value)
                : false;

            return new EventDto
            {
                Id = eventItem.Id,
                Title = eventItem.Title,
                Description = eventItem.Description,
                Date = eventItem.Date,
                Location = eventItem.Location,
                Capacity = eventItem.Capacity,
                AvailableSpots = availableSpots,
                IsBookedByCurrentUser = isBookedByCurrentUser
            };
        }

        public async Task<Event> CreateEventAsync(Event eventItem)
        {
            return await _eventRepository.AddAsync(eventItem);
        }

        public async Task<Event> UpdateEventAsync(Event eventItem)
        {
            return await _eventRepository.UpdateAsync(eventItem);
        }

        public async Task<bool> DeleteEventAsync(Guid id)
        {
            return await _eventRepository.DeleteAsync(id);
        }

        public async Task<int> GetAvailableSpotsAsync(Guid eventId)
        {
            var eventItem = await _eventRepository.GetByIdAsync(eventId);
            if (eventItem == null) return 0;

            var bookingCount = await _eventRepository.GetBookingCountAsync(eventId);
            return Math.Max(0, eventItem.Capacity - bookingCount);
        }
    }
}
