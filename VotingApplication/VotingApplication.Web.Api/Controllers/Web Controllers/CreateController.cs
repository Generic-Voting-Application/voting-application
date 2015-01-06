using System.Web.Mvc;

namespace VotingApplication.Web.Api.Controllers
{
    public class CreateController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Create a poll";

            return View();
        }
    }
}
