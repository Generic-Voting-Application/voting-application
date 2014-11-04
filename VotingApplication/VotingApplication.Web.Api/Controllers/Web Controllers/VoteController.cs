using System.Web.Mvc;

namespace VotingApplication.Web.Api.Controllers.Web_Controllers
{
    public class VoteController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Voting";
            ViewBag.Script = "/Scripts/VoteAppPages/Vote.js";

            return View();
        }
    }
}
