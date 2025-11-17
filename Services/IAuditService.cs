namespace New_LeRayBookingSystem.Services
{
    public interface IAuditService
    {
        Task LogAsync(string action, string entityType, string entityId, string details, string module, string description);
    }
}
