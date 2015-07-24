using System.Web.Mvc;

namespace VotingApplication.Web.Controllers
{
    public class DashboardController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult MyPolls()
        {
            return View();
        }

        public ActionResult NotLoggedIn()
        {
            return View();
        }
    }
}