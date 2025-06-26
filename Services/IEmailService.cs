using PersonaXFleet.Models;

namespace PersonaXFleet.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(EmailMessage message);

        Task SendMaintenanceRequestEmailAsync(string userEmail, string action, MaintenanceRequest request);
    }

}
