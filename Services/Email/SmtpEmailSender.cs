using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace E_CommerceSystem.Services.Email
{
    public sealed class SmtpEmailSender : IEmailSender
    {
        private readonly EmailSettings _cfg;
        public SmtpEmailSender(IOptions<EmailSettings> cfg) => _cfg = cfg.Value;

        public async Task SendAsync(string toEmail, string subject, string htmlBody, byte[]? attachment = null, string? attachmentName = null)
        {
            using var msg = new MailMessage();
            msg.From = new MailAddress(_cfg.FromEmail, _cfg.FromName);
            msg.To.Add(new MailAddress(toEmail));
            msg.Subject = subject;
            msg.Body = htmlBody;
            msg.IsBodyHtml = true;

            if (attachment is not null && attachmentName is not null)
                msg.Attachments.Add(new Attachment(new System.IO.MemoryStream(attachment), attachmentName));

            using var client = new SmtpClient(_cfg.Host, _cfg.Port)
            {
                EnableSsl = _cfg.EnableSsl,
                Credentials = new NetworkCredential(_cfg.UserName, _cfg.Password)
            };

            await client.SendMailAsync(msg);
        }
    }
}
