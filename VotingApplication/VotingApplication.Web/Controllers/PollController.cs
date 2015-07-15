using System.Web.Mvc;

namespace VotingApplication.Web.Controllers
{
    public class PollController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Vote()
        {
            return View();
        }

        public ActionResult Results()
        {
            return View();
        }

        public ActionResult InviteOnly()
        {
            return View();
        }
    }
}