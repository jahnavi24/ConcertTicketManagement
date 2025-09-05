using Microsoft.EntityFrameworkCore;
using BackendServiceDemo.Models;

namespace BackendServiceDemo.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Event> Events { get; set; }
        public DbSet<TicketType> TicketTypes { get; set; }
        public DbSet<Ticket> Tickets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Event>().HasMany(e => e.TicketTypes).WithOne().HasForeignKey(t => t.EventId);

            modelBuilder.Entity<TicketType>(entity =>
            {
                entity.ToTable(tb =>
                {
                    tb.HasCheckConstraint("CK_TicketType_BookedCount", "BookedCount <= Capacity");
                });
            });
        }
    }
}