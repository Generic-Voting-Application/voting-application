using System.Web.Mvc;

namespace VotingApplication.Web.Api.Controllers
{
    public class ManageController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Manage your poll";

            return View();
        }
    }
}
