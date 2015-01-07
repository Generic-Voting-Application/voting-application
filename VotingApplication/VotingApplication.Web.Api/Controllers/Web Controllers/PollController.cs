using System;
using System.Web.Mvc;

namespace VotingApplication.Web.Api.Controllers
{
    public class PollController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Cast your vote";

            return View();
        }
    }
}
