namespace BackendServiceDemo.Services
{
    public interface ITicketReservationService
    {
        Task CleanupExpiredReservationsAsync();
    }
}
