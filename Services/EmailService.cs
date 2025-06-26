﻿    using MailKit.Net.Smtp;
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

        public EmailService(IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
            _razorEngine = new RazorLightEngineBuilder()
                .UseEmbeddedResourcesProject(typeof(Program))
                .UseMemoryCachingProvider()
                .Build();
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

        public async Task SendMaintenanceRequestEmailAsync(string userEmail, string action, MaintenanceRequest request)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            email.To.Add(new MailboxAddress("", userEmail));
            email.Subject = $"Maintenance Request {action}";

            var body = $"The maintenance request {request.MaintenanceId} has been {action}.";
            var bodyBuilder = new BodyBuilder { HtmlBody = body };
            email.Body = bodyBuilder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_settings.SmtpServer, _settings.Port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_settings.Username, _settings.Password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}

