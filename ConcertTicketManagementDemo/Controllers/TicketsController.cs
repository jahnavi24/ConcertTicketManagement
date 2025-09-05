using BackendServiceDemo.Data;
using BackendServiceDemo.Models;
using BackendServiceDemo.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendServiceDemo.Controllers 
{
    [Route("api/tickets")]
    [ApiController]
    public class TicketController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ITicketReservationService _reservationService;

        public TicketController(AppDbContext context, ITicketReservationService reservationService)
        {
            _context = context;
            _reservationService = reservationService;
        }

        [HttpPost("reserve")]
        public async Task<IActionResult> ReserveTicket(ReservationRequest reservation)
        {
            // 1. Cleanup expired reservations (ideally in background service, but here for now)
            await _reservationService.CleanupExpiredReservationsAsync();

            // 2. Validate ticket type
            var ticketType = await _context.TicketTypes
                .FirstOrDefaultAsync(tt => tt.TicketTypeId == reservation.TicketTypeId);

            if (ticketType is null)
                return NotFound("Ticket type not found.");

            // 3. Enforce single active reservation per customer (idempotent)
            var existingReservation = await _context.Tickets
                .FirstOrDefaultAsync(t =>
                    t.TicketTypeId == reservation.TicketTypeId &&
                    t.CustomerEmail == reservation.CustomerEmail &&
                    t.Status == TicketStatus.Reserved);

            if (existingReservation != null)
                return Ok(existingReservation);

            // 4. Capacity check
            if (ticketType.BookedCount >= ticketType.Capacity)
                return BadRequest("No tickets available.");

            // 5. Increment BookedCount and create reservation
            ticketType.BookedCount++;

            var ticket = new Ticket
            {
                TicketId = Guid.NewGuid(),
                TicketTypeId = ticketType.TicketTypeId,
                EventId = ticketType.EventId,
                Status = TicketStatus.Reserved,
                ReservationExpiry = DateTime.UtcNow.AddMinutes(15), // reservation window
                CustomerEmail = reservation.CustomerEmail
            };

            _context.Tickets.Add(ticket);

            try
            {
                await _context.SaveChangesAsync();
                return Ok(ticket);
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict("Reservation failed due to concurrent updates. Please try again.");
            }
        }

        [HttpPost("purchase")]
        public async Task<IActionResult> PurchaseTicket(TicketPurchaseRequest purchaseRequest)
        {
            var ticket = await _context.Tickets.FindAsync(purchaseRequest.TicketId);
            if (ticket == null) return NotFound("Ticket not found.");

            if (ticket.CustomerEmail != purchaseRequest.CustomerEmail)
                return BadRequest("Customer email does not match the reservation.");

            if (ticket.Status == TicketStatus.Purchased)
                return Ok(ticket); // idempotent response

            if (ticket.Status != TicketStatus.Reserved)
                return BadRequest("Ticket is not reserved.");

            // Optional: verify reservation hasn't expired before purchase
            if (ticket.ReservationExpiry.HasValue && ticket.ReservationExpiry < DateTime.UtcNow)
                return BadRequest("Reservation expired. Please reserve again.");

            // Verify paymentTransactionId with payment processing system
            // if(!PaymentService.VerifyTransaction(purchaseRequest.PaymentTransactionId)) { 
            //     return BadRequest("Transaction verification failed"); 
            // }

            ticket.Status = TicketStatus.Purchased;
            ticket.PurchaseDate = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(ticket);
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict("Purchase failed due to concurrent updates. Please try again.");
            }
        }

        [HttpPost("cancel")]
        public async Task<IActionResult> CancelTicket(TicketCancellationRequest cancellationRequest)
        {
            var ticket = await _context.Tickets.FindAsync(cancellationRequest.TicketId);
            if (ticket == null) return NotFound();

            if (ticket.CustomerEmail != cancellationRequest.CustomerEmail)
                return BadRequest("Customer email does not match the reservation.");

            if (ticket.Status == TicketStatus.Cancelled)
                return BadRequest("Ticket already cancelled.");

            // Only decrement if the ticket was Reserved or Purchased
            if (ticket.Status == TicketStatus.Reserved || ticket.Status == TicketStatus.Purchased)
            {
                var ticketType = await _context.TicketTypes.FindAsync(ticket.TicketTypeId);
                if (ticketType != null && ticketType.BookedCount > 0)
                {
                    ticketType.BookedCount--; // free up space
                }
            }

            ticket.Status = TicketStatus.Cancelled;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(ticket);
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict("Cancellation failed due to concurrent updates. Please try again.");
            }
        }

        [HttpGet("availability/{eventId}")]
        public async Task<IActionResult> CheckAvailability(Guid eventId)
        {
            // Cleanup expired reservations before calculating availability
            await _reservationService.CleanupExpiredReservationsAsync();

            // Fetch ticket types with booked count
            var ticketTypes = await _context.TicketTypes
                .Where(t => t.EventId == eventId)
                .Select(t => new
                {
                    t.TicketTypeId,
                    t.Name,
                    t.Capacity,
                    t.BookedCount
                })
                .ToListAsync();

            // Calculate available tickets
            var result = ticketTypes.Select(t => new
            {
                t.TicketTypeId,
                t.Name,
                t.Capacity,
                BookedTickets = t.BookedCount,
                AvailableTickets = Math.Max(0, t.Capacity - t.BookedCount)
            });

            return Ok(result);
        }
    }
}
