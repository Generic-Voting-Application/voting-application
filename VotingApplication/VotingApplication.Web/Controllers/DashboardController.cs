using System.Web.Mvc;

namespace VotingApplication.Web.Controllers
{
    public class DashboardController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}