using BackendServiceDemo.Data;
using BackendServiceDemo.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1.Register services
builder.Services.AddControllers();
builder.Services.AddScoped<ITicketReservationService, TicketReservationService>();

// Register SQLite DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=EventManagementDatabase.db"));

// Add Swagger/OpenAPI support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 2.Configure Kestrel for HTTP + HTTPS
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5159); // HTTP
    options.ListenLocalhost(7170, listenOptions =>
    {
        listenOptions.UseHttps(); // HTTPS
    });
});

var app = builder.Build();

// 3.Ensure database and tables exist
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated(); // Creates tables if they don't exist
}

// 4.Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
