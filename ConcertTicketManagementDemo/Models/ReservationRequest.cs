using System.ComponentModel.DataAnnotations;

namespace BackendServiceDemo.Models
{
    public class ReservationRequest
    {
        [Required]
        public Guid TicketTypeId { get; set; }

        // In production scenarios, this should be populated by AuthN middleware. Taking as input for now.
        [Required]
        [EmailAddress]
        public string CustomerEmail { get; set; } = string.Empty;

    }
}
