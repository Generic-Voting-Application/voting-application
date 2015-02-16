using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VotingApplication.Web.Api.Controllers.API_Controllers
{
    public interface IMailSender
    {
        void SendMail(string to, string subject, string message);
    }
}
