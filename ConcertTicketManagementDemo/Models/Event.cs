using System;
using System.ComponentModel.DataAnnotations;

namespace BackendServiceDemo.Models {
    public class Event
    {
        [Key]
        public Guid EventId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Venue { get; set; } = string.Empty; 

        [Required]
        public DateTime EventDate { get; set; }

        public ICollection<TicketType> TicketTypes { get; set; } = new List<TicketType>();
    }
}
