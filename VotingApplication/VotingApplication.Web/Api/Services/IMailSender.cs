using System.Threading.Tasks;

namespace VotingApplication.Web.Api.Services
{
    public interface IMailSender
    {
        Task SendMail(string to, string subject, string message);
    }
}
