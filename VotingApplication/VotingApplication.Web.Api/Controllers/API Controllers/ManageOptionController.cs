using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;

namespace VotingApplication.Web.Api.Controllers.API_Controllers
{
    public class ManageOptionController : WebApiController
    {
        public ManageOptionController() : base() {}
        public ManageOptionController(IContextFactory contextFactory) : base(contextFactory) { }

        #region GET

        public virtual HttpResponseMessage Get(Guid manageId)
        {
            using (var context = _contextFactory.CreateContext())
            {
                Poll matchingPoll = context.Polls.Where(s => s.ManageID == manageId).Include(s => s.Options).FirstOrDefault();
                if (matchingPoll == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("Poll {0} not found", manageId));
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, matchingPoll.Options);
            }
        }

        public virtual HttpResponseMessage Get(Guid manageId, long voteId)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use GET by id on this controller");
        }

        #endregion

        #region POST

        public virtual HttpResponseMessage Post(Guid manageId, Option option)
        {
            using (var context = _contextFactory.CreateContext())
            {
                if (option.Name == null || option.Name == "")
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Option name must not be empty");

                }

                Poll matchingPoll = context.Polls.Where(s => s.ManageID == manageId).Include(s => s.Options).FirstOrDefault();
                if (matchingPoll == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("Poll {0} not found", manageId));
                }

                if (option.Polls == null)
                {
                    option.Polls = new List<Poll>();
                }

                matchingPoll.Options.Add(option);
                option.Polls.Add(matchingPoll);

                context.Options.Add(option);
                context.SaveChanges();

                return this.Request.CreateResponse(HttpStatusCode.OK, option.Id);
            }
        }

        public virtual HttpResponseMessage Post(Guid manageId, long voteId, Option option)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use POST by id on this controller");
        }

        #endregion

        #region PUT

        public virtual HttpResponseMessage Put(Guid manageId, Option option)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use PUT on this controller");
        }

        public virtual HttpResponseMessage Put(Guid manageId, long voteId, Option option)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use PUT by id on this controller");
        }

        #endregion

        #region DELETE

        public virtual HttpResponseMessage Delete(Guid manageId)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use DELETE on this controller");
        }

        public virtual HttpResponseMessage Delete(Guid manageId, long optionId)
        {
            using (var context = _contextFactory.CreateContext())
            {
                Poll matchingPoll = context.Polls.Where(s => s.ManageID == manageId).Include(s => s.Options).FirstOrDefault();
                if (matchingPoll == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("Poll {0} not found", manageId));
                }

                Option matchingOption = matchingPoll.Options.Where(o => o.Id == optionId).FirstOrDefault();
                if (matchingOption != null)
                {
                    matchingPoll.Options.Remove(matchingOption);

                    // Remove votes for this option/poll combo
                    List<Vote> optionVotes = context.Votes.Where(v => v.OptionId == optionId && v.PollId == matchingPoll.UUID).ToList();
                    foreach (Vote vote in optionVotes)
                    {
                        context.Votes.Remove(vote);
                    }
                }

                context.SaveChanges();

                return this.Request.CreateResponse(HttpStatusCode.OK);
            }
        }

        #endregion
    }
}