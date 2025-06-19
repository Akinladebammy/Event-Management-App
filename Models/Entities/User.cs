using EventManagementApp.Models.Enum;
using System.ComponentModel.DataAnnotations;

namespace EventManagementApp.Models.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email {get; set; }
        public string PasswordHash { get; set; } = string.Empty;
        public Role Role { get; set; }
        public ICollection<EventBooking> Bookings { get; set; }

    }
}
