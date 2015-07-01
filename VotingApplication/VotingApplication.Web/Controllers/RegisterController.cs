using System.Web.Mvc;

namespace VotingApplication.Web.Controllers
{
    public class RegisterController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Register()
        {
            return View();
        }

        public ActionResult RegistrationComplete()
        {
            return View();
        }
    }
}