using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Services.Contracts;
using Microsoft.Extensions.Options;
using Entity.ModelsDto;

namespace Services.EFCore
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _smtpSettings;

        public EmailService(IOptions<SmtpSettings> smtpSettings)
        {
            _smtpSettings = smtpSettings.Value;
        }
        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            try
            {
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_smtpSettings.SenderEmail, _smtpSettings.SenderName),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                using var client = new SmtpClient(_smtpSettings.Server, _smtpSettings.Port)
                {
                    Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password),
                    EnableSsl = _smtpSettings.UseSSL
                };

                await client.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"E-posta gönderimi başarısız oldu: {ex.Message}");
            }
        }
    }
}
