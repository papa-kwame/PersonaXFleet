using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PersonaXFleet.Models;
using PersonaXFleet.Services;
using System.Text;

namespace PersonaXFleet.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MailController : ControllerBase
    {
        private readonly IEmailService _email;
        private readonly IWebHostEnvironment _env;

        public MailController(IEmailService email, IWebHostEnvironment env)
        {
            _email = email;
            _env = env;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendEmail(EmailMessage message)
        {
            var placeholders = new Dictionary<string, string>
            {
                { "Subject", message.Subject },
                { "BodyContent", message.Body },
                { "Year", DateTime.Now.Year.ToString() }
            };

            string template = LoadTemplate("Templates/EmailTemplate.html", placeholders);
            message.Body = template;

            await _email.SendEmailAsync(message);
            return Ok("Sent!");
        }

        [HttpGet("test")]
        public async Task<IActionResult> SendTest()
        {
            var placeholders = new Dictionary<string, string>
            {
                { "Subject", "Test Email" },
                { "BodyContent", "<p>This is a test from your magical email service with a beautiful template!</p>" },
                { "Year", DateTime.Now.Year.ToString() }
            };

            string template = LoadTemplate("Templates/EmailTemplate.html", placeholders);

            var msg = new EmailMessage
            {
                ToAddresses = new() { "pkwork1204@gmail.com" },
                Subject = placeholders["Subject"],
                Body = template
            };

            await _email.SendEmailAsync(msg);
            return Ok("Email with template sent!");
        }

        private string LoadTemplate(string relativePath, Dictionary<string, string> placeholders)
        {
            // Use the content root to correctly locate files in the project directory
            string path = Path.Combine(_env.ContentRootPath, relativePath);

            if (!System.IO.File.Exists(path))
            {
                Console.WriteLine($"Template not found at: {path}");
                return "<p>Email template not found.</p>";
            }

            string template = System.IO.File.ReadAllText(path, Encoding.UTF8);
            foreach (var placeholder in placeholders)
            {
                template = template.Replace($"{{{{{placeholder.Key}}}}}", placeholder.Value);
            }

            return template;
        }
    }
}