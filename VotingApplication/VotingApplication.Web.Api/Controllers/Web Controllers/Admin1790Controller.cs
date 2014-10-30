﻿using System.Web.Mvc;

namespace VotingApplication.Web.Api.Controllers.Web_Controllers
{
    public class Admin1790Controller : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Admin Panel";
            ViewBag.Script = "/Scripts/VoteAppPages/Admin.js";

            return View();
        }
    }
}
