using System.Web.Mvc;
using VotingApplication.Web.Api.Models;

namespace VotingApplication.Web.Api.Controllers
{
    public class ManageController : Controller
    {
        public ActionResult Index(ManageModel model)
        {
            ViewBag.Title = "Manage your poll";

            return View(model);
        }
    }
}
