using System;
using System.Web.Mvc;
using VotingApplication.Web.Api.Models;

namespace VotingApplication.Web.Api.Controllers
{
    public class PollController : Controller
    {
        public ActionResult Index(PollModel model)
        {
            ViewBag.Title = "Cast your vote";

            return View(model);
        }
    }
}
