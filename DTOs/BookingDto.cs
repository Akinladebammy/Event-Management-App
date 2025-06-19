namespace EventManagementApp.DTOs
{
    namespace EventManagementApp.DTOs
    {
        public class BookingDto
        {
            public Guid Id { get; set; }
            public Guid EventId { get; set; }
            public Guid UserId { get; set; }
            public DateTime BookingDate { get; set; }

            // Event details for display
            public string EventTitle { get; set; }
            public string EventDescription { get; set; }
            public DateTime EventDate { get; set; }
            public string EventLocation { get; set; }
            public int EventCapacity { get; set; }

            // User details for display (if needed)
            public string Username { get; set; }
            public string UserEmail { get; set; }

            // Additional properties for UI
            public bool CanCancel { get; set; } // Based on event date or business rules
            public string Status { get; set; } // Active, Cancelled, Past, etc.
        }
    }
}
