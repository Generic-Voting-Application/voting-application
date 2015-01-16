using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace VotingApplication.Web.Api.Controllers
{
    public class SignInController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Sign In or Register";

            return View();
        }
    }
}
