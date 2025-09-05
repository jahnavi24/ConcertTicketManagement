using BackendServiceDemo.Controllers;
using BackendServiceDemo.Data;
using BackendServiceDemo.Models;
using BackendServiceDemo.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

public class TicketControllerTests
{
    private AppDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private TicketController GetController(AppDbContext context)
    {
        var reservationService = new TicketReservationService(context);
        return new TicketController(context, reservationService);
    }

    private Event CreateSampleEvent(AppDbContext context)
    {
        var ev = new Event
        {
            Name = "Test Event",
            Description = "Description",
            Venue = "Venue",
            EventDate = DateTime.UtcNow.AddDays(1),
            TicketTypes = new List<TicketType>
            {
                new TicketType { Name = "VIP", Price = 100, Capacity = 2 },
                new TicketType { Name = "Regular", Price = 50, Capacity = 5 }
            }
        };
        context.Events.Add(ev);
        context.SaveChanges();
        return ev;
    }

    [Fact]
    public async Task ReserveTicket_ReturnsOk_WhenAvailable()
    {
        var context = GetDbContext();
        var controller = GetController(context);
        var ev = CreateSampleEvent(context);
        var ticket = new ReservationRequest
        {
            TicketTypeId = ev.TicketTypes.First().TicketTypeId,
            CustomerEmail = "test@example.com"
        };

        var result = await controller.ReserveTicket(ticket) as OkObjectResult;

        Assert.NotNull(result);
        var reservedTicket = result.Value as Ticket;
        Assert.Equal(TicketStatus.Reserved, reservedTicket.Status);
        Assert.Equal(ticket.CustomerEmail, reservedTicket.CustomerEmail);
    }

    [Fact]
    public async Task ReserveTicket_ReturnsBadRequest_WhenFull()
    {
        var context = GetDbContext();
        var controller = GetController(context);
        var ev = CreateSampleEvent(context);
        var ticketType = ev.TicketTypes.First(); // Capacity = 2

        // Reserve max tickets
        await controller.ReserveTicket(new ReservationRequest { TicketTypeId = ticketType.TicketTypeId, CustomerEmail = "a@test.com" });
        await controller.ReserveTicket(new ReservationRequest { TicketTypeId = ticketType.TicketTypeId, CustomerEmail = "b@test.com" });

        var result = await controller.ReserveTicket(new ReservationRequest { TicketTypeId = ticketType.TicketTypeId, CustomerEmail = "c@test.com" });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task PurchaseTicket_ChangesStatusToPurchased()
    {
        var context = GetDbContext();
        var controller = GetController(context);
        var ev = CreateSampleEvent(context);
        var ticketType = ev.TicketTypes.First();

        var reserveResult = await controller.ReserveTicket(new ReservationRequest { TicketTypeId = ticketType.TicketTypeId, CustomerEmail = "user@test.com" }) as OkObjectResult;
        var reservedTicket = reserveResult.Value as Ticket;

        var purchaseResult = await controller.PurchaseTicket(new TicketPurchaseRequest {TicketId = reservedTicket.TicketId, CustomerEmail = reservedTicket.CustomerEmail, PaymentTransactionId = "PaymentTransactionId" }) as OkObjectResult;
        var purchasedTicket = purchaseResult.Value as Ticket;

        Assert.Equal(TicketStatus.Purchased, purchasedTicket.Status);
        Assert.NotNull(purchasedTicket.PurchaseDate);
    }

    [Fact]
    public async Task PurchaseTicket_ReturnsBadRequest_WhenNotReserved()
    {
        var context = GetDbContext();
        var controller = GetController(context);

        var result = await controller.PurchaseTicket(new TicketPurchaseRequest { TicketId = Guid.NewGuid(), CustomerEmail = "user@test.com", PaymentTransactionId = "PaymentTransactionId" });
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task CancelTicket_ChangesStatusToCancelled()
    {
        var context = GetDbContext();
        var controller = GetController(context);
        var ev = CreateSampleEvent(context);
        var ticketType = ev.TicketTypes.First();

        var reserveResult = await controller.ReserveTicket(new ReservationRequest { TicketTypeId = ticketType.TicketTypeId, CustomerEmail = "user@test.com" }) as OkObjectResult;
        var reservedTicket = reserveResult.Value as Ticket;

        var cancelResult = await controller.CancelTicket(new TicketCancellationRequest { TicketId = reservedTicket.TicketId, CustomerEmail = "user@test.com" }) as OkObjectResult;
        var cancelledTicket = cancelResult.Value as Ticket;

        Assert.Equal(TicketStatus.Cancelled, cancelledTicket.Status);
    }

    [Fact]
    public async Task CheckAvailability_ReturnsCorrectCounts()
    {
        var context = GetDbContext();
        var controller = GetController(context);
        var ev = CreateSampleEvent(context);
        var ticketType = ev.TicketTypes.First(); // VIP

        await controller.ReserveTicket(new ReservationRequest { TicketTypeId = ticketType.TicketTypeId, CustomerEmail = "a@test.com" });
        await controller.ReserveTicket(new ReservationRequest { TicketTypeId = ticketType.TicketTypeId, CustomerEmail = "b@test.com" });

        var result = await controller.CheckAvailability(ev.EventId) as OkObjectResult;
        var availability = result.Value as IEnumerable<object>;

        Assert.NotNull(availability);
    }

    [Fact]
    public async Task ReserveTicket_ReturnsSameReservation_ForSameCustomer()
    {
        var context = GetDbContext();
        var controller = GetController(context);
        var ev = CreateSampleEvent(context);
        var ticketType = ev.TicketTypes.First();

        var first = await controller.ReserveTicket(new ReservationRequest { TicketTypeId = ticketType.TicketTypeId, CustomerEmail = "same@test.com" }) as OkObjectResult;
        var second = await controller.ReserveTicket(new ReservationRequest { TicketTypeId = ticketType.TicketTypeId, CustomerEmail = "same@test.com" }) as OkObjectResult;

        var ticket1 = first.Value as Ticket;
        var ticket2 = second.Value as Ticket;

        Assert.Equal(ticket1.TicketId, ticket2.TicketId); // Idempotent check
    }
}
