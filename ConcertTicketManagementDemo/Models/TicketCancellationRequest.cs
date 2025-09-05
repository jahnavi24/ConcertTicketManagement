using System.ComponentModel.DataAnnotations;

namespace BackendServiceDemo.Models
{
    public class TicketCancellationRequest
    {
        [Required]
        public Guid TicketId { get; set; }

        [Required]
        [EmailAddress]
        public string CustomerEmail { get; set; } = string.Empty;
    }
}
