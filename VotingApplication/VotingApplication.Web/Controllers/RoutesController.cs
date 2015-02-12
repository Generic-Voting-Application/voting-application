using System.Web.Mvc;

namespace VotingApplication.Web.Controllers
{
    public class RoutesController : Controller
    {
        public ActionResult Voting()
        {
            return View();
        }

        public ActionResult Results()
        {
            return View();
        }
    }
}