using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendServiceDemo.Models
{
    public class Ticket
    {
        [Key]
        public Guid TicketId { get; set; }

        [Required]
        public Guid EventId { get; set; }

        [ForeignKey("EventId")]
        public Event? Event { get; set; }

        [Required]
        public Guid TicketTypeId { get; set; }

        [ForeignKey("TicketTypeId")]
        public TicketType TicketType { get; set; } = null!;

        [Required]
        public TicketStatus Status { get; set; }

        public DateTime? ReservationExpiry { get; set; }

        public DateTime? PurchaseDate { get; set; }

        [Required]
        [EmailAddress]
        public string CustomerEmail { get; set; } = string.Empty;
    }

    public enum TicketStatus
    {
        Reserved = 0,
        Purchased = 1,
        Cancelled = 2
    }
}
