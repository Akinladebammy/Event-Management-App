namespace EventManagementApp.DTOs
{
    public class EventDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public string Location { get; set; }
        public int Capacity { get; set; }
        public int AvailableSpots { get; set; }
        public bool IsBookedByCurrentUser { get; set; }
    }
}
