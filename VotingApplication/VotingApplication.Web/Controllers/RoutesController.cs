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

        public ActionResult BasicVote()
        {
            return View();
        }

        public ActionResult PointsVote()
        {
            return View();
        }

        public ActionResult UpDownVote()
        {
            return View();
        }


        public ActionResult LoginDialog()
        {
            return View();
        }

        public ActionResult PollHeading()
        {
            return View();
        }

        public ActionResult Create()
        {
            return View();
        }

        public ActionResult Home()
        {
            return View();
        }
    }
}
