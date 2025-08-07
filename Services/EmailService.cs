    using MailKit.Net.Smtp;
    using Microsoft.Extensions.Options;
    using MimeKit;
    using PersonaXFleet.Config;
    using PersonaXFleet.Models;
    using RazorLight;
     using MailKit.Security;


namespace PersonaXFleet.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly RazorLightEngine _razorEngine;
        private readonly IBackgroundTaskQueue _taskQueue;

        public EmailService(IOptions<EmailSettings> settings, IBackgroundTaskQueue taskQueue    )
        {
            _settings = settings.Value;
            _razorEngine = new RazorLightEngineBuilder()
                .UseEmbeddedResourcesProject(typeof(Program))
                .UseMemoryCachingProvider()
                .Build();
            _taskQueue = taskQueue;
        }

        public async Task SendEmailAsync(EmailMessage message)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            email.To.AddRange(message.ToAddresses.Select(addr => MailboxAddress.Parse(addr)));
            email.Subject = message.Subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = message.Body };
            email.Body = bodyBuilder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_settings.SmtpServer, _settings.Port, SecureSocketOptions.StartTls);

            await smtp.AuthenticateAsync(_settings.Username, _settings.Password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

        public async Task SendMaintenanceRequestEmailAsync(string userEmail, string userFirstName, string action, MaintenanceRequest request)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            email.To.Add(new MailboxAddress("", userEmail));
            email.Subject = $"Maintenance Request {action}";

            var model = new
            {
                firstName = userFirstName, 
                BodyContent = $"The maintenance request {request.MaintenanceId} has been {action}.",
                Year = DateTime.Now.Year
            };

            var body = await _razorEngine.CompileRenderAsync("EmailTemplate.cshtml", model);

            var bodyBuilder = new BodyBuilder { HtmlBody = body };
            email.Body = bodyBuilder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_settings.SmtpServer, _settings.Port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_settings.Username, _settings.Password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

        public Task SendMaintenanceRequestEmailAsync(string userEmail, string action, MaintenanceRequest request)
        {
            throw new NotImplementedException();
        }
    }
}

