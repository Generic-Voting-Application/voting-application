using System;
using System.Web.Mvc;
using VotingApplication.Data.Context;
using VotingApplication.Web.Api.Metrics;

namespace VotingApplication.Web.Controllers
{
    public class RoutesController : Controller
    {
        private IMetricHandler _metricHandler;

        public RoutesController()
        {
            var ContextFactory = new ContextFactory();
            _metricHandler = new MetricHandler(ContextFactory);
        }

        public RoutesController(IMetricHandler metricHandler)
        {
            _metricHandler = metricHandler ?? new EmptyMetricHandler();
        }

        private void LogPageEvent(string route)
        {
            object pollId = RouteData.Values["id"] ?? Guid.Empty;
            _metricHandler.HandlePageChangeEvent(route, Response.StatusCode, Guid.Parse(pollId.ToString()));
        }

        public ActionResult Vote()
        {
            LogPageEvent("Vote");
            return View();
        }

        public ActionResult Results()
        {
            LogPageEvent("Results");
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

        public ActionResult MultiVote()
        {
            return View();
        }

        public ActionResult IdentityLogin()
        {
            return View();
        }

        public ActionResult PollHeading()
        {
            return View();
        }

        public ActionResult AccountLogin()
        {
            LogPageEvent("Login");
            return View();
        }

        public ActionResult AccountRegister()
        {
            LogPageEvent("Register");
            return View();
        }

        public ActionResult AccountResetPassword()
        {
            return View();
        }

        public ActionResult Manage()
        {
            LogPageEvent("Manage");
            return View();
        }

        public ActionResult ManageChoices()
        {
            LogPageEvent("ManageOptions");
            return View();
        }

        public ActionResult ManageInvitees()
        {
            LogPageEvent("ManageInvitees");
            return View();
        }

        public ActionResult ManageMisc()
        {
            LogPageEvent("ManageInvitationStyle");
            return View();
        }

        public ActionResult ManagePollType()
        {
            LogPageEvent("ManagePollType");
            return View();
        }

        public ActionResult ManageExpiry()
        {
            LogPageEvent("ManageExpiry");
            return View();
        }

        public ActionResult ManageVoters()
        {
            LogPageEvent("ManageVoters");
            return View();
        }

        public ActionResult PollTypeChange()
        {
            return View();
        }

        public ActionResult MyPolls()
        {
            LogPageEvent("MyPolls");
            return View();
        }

        public ActionResult DatePicker()
        {
            return View();
        }

        public ActionResult TimePicker()
        {
            return View();
        }

        public ActionResult Create()
        {
            return View();
        }

        public ActionResult ErrorBar()
        {
            return View();
        }

        public ActionResult AddChoiceDialog()
        {
            return View();
        }

        public ActionResult EditChoiceDialog()
        {
            return View();
        }

        public ActionResult ConfirmRegistration()
        {
            return View();
        }
    }
}
