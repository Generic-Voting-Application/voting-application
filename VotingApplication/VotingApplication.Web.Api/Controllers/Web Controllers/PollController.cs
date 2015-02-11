using System;
using System.Linq;
using System.Web.Mvc;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Models;

namespace VotingApplication.Web.Api.Controllers
{
    public class PollController : Controller
    {
        private readonly IContextFactory _contextFactory;

        public PollController()
        {
            this._contextFactory = new ContextFactory();
        }

        public ActionResult Index(string id, string token)
        {
            ViewBag.Title = "Cast your vote";

            PollModel pollModel = new PollModel { Id = id, URITokenGuid = token, VotingStrategy = String.Empty };

            using (var context = _contextFactory.CreateContext())
            {
                Guid pollId;
                if (Guid.TryParse(id, out pollId))
                {
                    Poll poll = context.Polls.Where(p => p.UUID == pollId).FirstOrDefault();

                    if (poll != null)
                    {
                        pollModel.VotingStrategy = poll.PollType.ToString();
                    }
                }

            }

            return View(pollModel);
        }
    }
}
