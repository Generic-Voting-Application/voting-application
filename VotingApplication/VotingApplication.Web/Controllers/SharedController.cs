using System.Web.Mvc;

namespace VotingApplication.Web.Controllers
{
    public class SharedController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult NotFound()
        {
            return View();
        }

        public ActionResult GenericError()
        {
            return View();
        }
    }
}