using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendServiceDemo.Models {
    public class TicketType
    {
        [Key]
        public Guid TicketTypeId { get; set; }

        [Required]
        public Guid EventId { get; set; }

        [ForeignKey("EventId")]
        public Event? Event { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Required]
        public int Capacity { get; set; }

        // The current implementation for capacity management uses 'ConcurrencyCheck'.
        // Under high TPS scenarios, this might lead to frequent exceptions.
        // Using guarded transactions will mitigate this issue.
        [ConcurrencyCheck]
        public int BookedCount { get; set; } = 0;
    }
}
