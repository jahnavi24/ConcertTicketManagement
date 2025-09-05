using BackendServiceDemo.Controllers;  // EventController
using BackendServiceDemo.Data;         // AppDbContext
using BackendServiceDemo.Models;       // Event, TicketType
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;
using System.Collections.Generic;

public class EventControllerTests
{
    private AppDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private EventController GetController(AppDbContext context)
    {
        return new EventController(context);
    }

    private Event CreateSampleEvent()
    {
        return new Event
        {
            Name = "Sample Event",
            Description = "Test Event",
            Venue = "Test Venue",
            EventDate = DateTime.UtcNow.AddDays(1),
            TicketTypes = new List<TicketType>
            {
                new TicketType { Name = "VIP", Price = 100, Capacity = 10 },
                new TicketType { Name = "Regular", Price = 50, Capacity = 40 }
            }
        };
    }

    [Fact]
    public async Task CreateEvent_ReturnsCreatedEvent()
    {
        var context = GetDbContext();
        var controller = GetController(context);
        var newEvent = CreateSampleEvent();

        var result = await controller.Create(newEvent) as CreatedAtActionResult;

        Assert.NotNull(result);
        var createdEvent = result.Value as Event;
        Assert.Equal("Sample Event", createdEvent.Name);
        Assert.Equal(2, createdEvent.TicketTypes.Count);
    }

    [Fact]
    public async Task CreateEvent_ReturnsConflict_WhenDuplicate()
    {
        var context = GetDbContext();
        var controller = GetController(context);
        var newEvent = CreateSampleEvent();

        await controller.Create(newEvent);
        var duplicateResult = await controller.Create(newEvent);

        Assert.IsType<ConflictObjectResult>(duplicateResult);
    }

    [Fact]
    public async Task GetAll_ReturnsEvents()
    {
        var context = GetDbContext();
        var controller = GetController(context);
        await controller.Create(CreateSampleEvent());

        var result = await controller.GetAll() as OkObjectResult;
        var events = result.Value as List<Event>;

        Assert.NotNull(events);
        Assert.Single(events);
    }

    [Fact]
    public async Task GetById_ReturnsEvent_WhenExists()
    {
        var context = GetDbContext();
        var controller = GetController(context);
        var createdResult = await controller.Create(CreateSampleEvent()) as CreatedAtActionResult;
        var createdEvent = createdResult.Value as Event;

        var result = await controller.GetById(createdEvent.EventId) as OkObjectResult;
        var ev = result.Value as Event;

        Assert.NotNull(ev);
        Assert.Equal(createdEvent.EventId, ev.EventId);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenDoesNotExist()
    {
        var context = GetDbContext();
        var controller = GetController(context);

        var result = await controller.GetById(Guid.NewGuid());
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UpdateEvent_UpdatesEventProperties()
    {
        var context = GetDbContext();
        var controller = GetController(context);
        var createdResult = await controller.Create(CreateSampleEvent()) as CreatedAtActionResult;
        var createdEvent = createdResult.Value as Event;

        var updatedEvent = new Event
        {
            Name = "Updated Event",
            Description = "Updated Desc",
            Venue = "Updated Venue",
            EventDate = DateTime.UtcNow.AddDays(2),
        };

        var result = await controller.Update(createdEvent.EventId, updatedEvent);
        Assert.IsType<NoContentResult>(result);

        var fetched = await controller.GetById(createdEvent.EventId) as OkObjectResult;
        var ev = fetched.Value as Event;

        Assert.Equal("Updated Event", ev.Name);
        Assert.Equal("Updated Desc", ev.Description);
        Assert.Equal("Updated Venue", ev.Venue);
    }

    [Fact]
    public async Task UpdateEvent_ReturnsNotFound_WhenEventMissing()
    {
        var context = GetDbContext();
        var controller = GetController(context);

        var result = await controller.Update(Guid.NewGuid(), CreateSampleEvent());
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteEvent_RemovesEvent()
    {
        var context = GetDbContext();
        var controller = GetController(context);
        var createdResult = await controller.Create(CreateSampleEvent()) as CreatedAtActionResult;
        var createdEvent = createdResult.Value as Event;

        var deleteResult = await controller.Delete(createdEvent.EventId);
        Assert.IsType<NoContentResult>(deleteResult);

        var fetchResult = await controller.GetById(createdEvent.EventId);
        Assert.IsType<NotFoundResult>(fetchResult);
    }

    [Fact]
    public async Task DeleteEvent_ReturnsNotFound_WhenMissing()
    {
        var context = GetDbContext();
        var controller = GetController(context);

        var result = await controller.Delete(Guid.NewGuid());
        Assert.IsType<NotFoundResult>(result);
    }
}
