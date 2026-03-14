using System.Threading.Tasks;

namespace FeesManagementSystem.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
