using System.Web.Mvc;

namespace VotingApplication.Web.Api.Controllers
{
    public class InfoController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Voting Information";

            return View();
        }
    }
}
