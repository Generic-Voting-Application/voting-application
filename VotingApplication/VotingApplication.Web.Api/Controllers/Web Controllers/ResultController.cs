using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace VotingApplication.Web.Api.Controllers
{
    public class ResultController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Voting Results";
            ViewBag.Script = "/Scripts/Results.js";

            return View();
        }
    }
}