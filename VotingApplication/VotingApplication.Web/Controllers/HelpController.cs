using System.Web.Mvc;

namespace VotingApplication.Web.Controllers
{
    public class HelpController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult VotingBasic()
        {
            return View("~/Views/Help/Voting/VotingBasic.cshtml");
        }

        public ActionResult VotingPoints()
        {
            return View("~/Views/Help/Voting/VotingPoints.cshtml");
        }

        public ActionResult VotingUpDown()
        {
            return View("~/Views/Help/Voting/VotingUpDown.cshtml");
        }

        public ActionResult VotingMulti()
        {
            return View("~/Views/Help/Voting/VotingMulti.cshtml");
        }
    }
}