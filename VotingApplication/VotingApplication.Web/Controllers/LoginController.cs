using System.Web.Mvc;

namespace VotingApplication.Web.Controllers
{
    public class LoginController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Login()
        {
            return View();
        }
    }
}