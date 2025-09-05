using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendServiceDemo.Data;
using BackendServiceDemo.Models;

namespace BackendServiceDemo.Controllers 
{
    [Route("api/events")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EventController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var events = await _context.Events
                        .Include(e => e.TicketTypes) // load related ticket types
                        .ToListAsync();
            return Ok(events);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var ev = await _context.Events
                    .Include(e => e.TicketTypes) // include related ticket types
                    .FirstOrDefaultAsync(e => e.EventId == id);
            if (ev == null) return NotFound();
            return Ok(ev);
        }

        //idempotency on name + date + venue?
        [HttpPost]
        public async Task<IActionResult> Create(Event ev)
        {
            // Check for existing event (idempotency key: Name + Date + Venue)
            var existing = await _context.Events
                .FirstOrDefaultAsync(e => e.Name == ev.Name
                                       && e.EventDate == ev.EventDate
                                       && e.Venue == ev.Venue);

            if (existing != null)
            {
                return Conflict("An event with the same name, date, and venue already exists.");
            }

            // Assign a new GUID for the Event
            ev.EventId = Guid.NewGuid();

            if (ev.TicketTypes == null || !ev.TicketTypes.Any())
                return BadRequest("At least one ticket type is required.");

            foreach (var tt in ev.TicketTypes)
            {
                tt.TicketTypeId = Guid.NewGuid();
                tt.EventId = ev.EventId;
            }

            // Add event (and its ticket types via EF relationship) to the context
            _context.Events.Add(ev);

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = ev.EventId }, ev);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, Event ev)
        {
            var existing = await _context.Events.Include(e => e.TicketTypes).FirstOrDefaultAsync(e => e.EventId == id);
            if (existing == null) return NotFound();

            // Update basic event properties
            existing.Name = ev.Name;
            existing.Description = ev.Description;
            existing.Venue = ev.Venue;
            existing.EventDate = ev.EventDate;

            // Update ticket types if provided
            if (ev.TicketTypes != null)
            {
                foreach (var tt in ev.TicketTypes)
                {
                    // Find existing ticket type by ID
                    var existingTT = existing.TicketTypes.FirstOrDefault(x => x.TicketTypeId == tt.TicketTypeId);
                    if (existingTT != null)
                    {
                        // Update only price and capacity
                        existingTT.Price = tt.Price;
                        existingTT.Capacity = tt.Capacity;
                    }
                }
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existing = await _context.Events.FindAsync(id);
            if (existing == null) return NotFound();

            _context.Events.Remove(existing);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}

