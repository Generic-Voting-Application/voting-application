using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace VotingApplication.Web.Api.Controllers
{
    public class DecisionController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Login to Create";

            return View();
        }
    }
}