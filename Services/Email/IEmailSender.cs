using System.Threading.Tasks;

namespace E_CommerceSystem.Services.Email
{
    public interface IEmailSender
    {
        Task SendAsync(string toEmail, string subject, string htmlBody, byte[]? attachment = null, string? attachmentName = null);
    }
}
