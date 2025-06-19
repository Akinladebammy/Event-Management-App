﻿namespace EventManagementApp.Models.Entities
{
    public class EventBooking
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
        public Event Event { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }

        public DateTime BookingDate { get; set; }
        
    }
}
