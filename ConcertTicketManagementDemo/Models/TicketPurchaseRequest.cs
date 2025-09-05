using System.ComponentModel.DataAnnotations;

namespace BackendServiceDemo.Models
{
    public class TicketPurchaseRequest
    {
        [Required]
        public Guid TicketId { get; set; }

        // In production scenarios, this should be populated by AuthN middleware. Taking as input for now.
        [Required]
        public string CustomerEmail { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string PaymentTransactionId { get; set; } = string.Empty;
    }
}
