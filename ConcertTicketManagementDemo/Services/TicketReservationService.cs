using BackendServiceDemo.Data;
using BackendServiceDemo.Models;
using Microsoft.EntityFrameworkCore;

namespace BackendServiceDemo.Services
{
    public class TicketReservationService : ITicketReservationService
    {
        private readonly AppDbContext _context;

        public TicketReservationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task CleanupExpiredReservationsAsync()
        {
            var expiredReservations = await _context.Tickets
                .Where(t => t.Status == TicketStatus.Reserved &&
                            t.ReservationExpiry.HasValue &&
                            t.ReservationExpiry < DateTime.UtcNow)
                .ToListAsync();

            if (!expiredReservations.Any()) return;

            foreach (var expired in expiredReservations)
            {
                expired.Status = TicketStatus.Cancelled;

                var ticketTypeToUpdate = await _context.TicketTypes
                    .FirstOrDefaultAsync(tt => tt.TicketTypeId == expired.TicketTypeId);

                if (ticketTypeToUpdate is { BookedCount: > 0 })
                {
                    ticketTypeToUpdate.BookedCount--; // free up capacity
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
