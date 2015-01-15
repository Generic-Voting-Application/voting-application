using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Models.DBViewModels;

namespace VotingApplication.Web.Api.Controllers.API_Controllers
{
    public class ManageVoteController : WebApiController
    {
        public ManageVoteController() : base() { }
        public ManageVoteController(IContextFactory contextFactory) : base(contextFactory) { }

        #region GET

        public virtual HttpResponseMessage Get(Guid manageId)
        {
            #region DB Get / Validation

            Poll poll;
            List<Vote> votes;
            using (var context = _contextFactory.CreateContext())
            {
                poll = context.Polls.Where(s => s.ManageId == manageId).Include(s => s.Options).SingleOrDefault();

                if (poll == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("Poll {0} not found", manageId));
                }

                votes = context.Votes.Where(v => v.PollId == poll.UUID).ToList();
            }

            #endregion

            #region Response

            List<VoteRequestResponseModel> response = new List<VoteRequestResponseModel>();

            foreach (Vote vote in votes)
            {
                VoteRequestResponseModel responseVote = new VoteRequestResponseModel();

                if (vote.Option != null)
                {
                    responseVote.OptionId = vote.Option.Id;
                    responseVote.OptionName = vote.Option.Name;
                }

                if (vote.User != null)
                {
                    responseVote.VoterName = vote.User.Name;
                    responseVote.UserId = vote.User.Id;
                }

                responseVote.VoteValue = vote.PollValue;

                response.Add(responseVote);
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, response);
            #endregion
        }

        public virtual HttpResponseMessage Get(Guid manageId, long voteId)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use GET by id on this controller");
        }

        #endregion

        #region POST

        public virtual HttpResponseMessage Post(Guid manageId, Vote vote)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use POST on this controller");
        }

        public virtual HttpResponseMessage Post(Guid manageId, long voteId, Vote vote)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use POST by id on this controller");
        }

        #endregion

        #region PUT

        public virtual HttpResponseMessage Put(Guid manageId, Vote vote)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use PUT on this controller");
        }

        public virtual HttpResponseMessage Put(Guid manageId, long voteId, Vote vote)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use PUT by id on this controller");
        }

        #endregion

        #region DELETE

        public virtual HttpResponseMessage Delete(Guid manageId)
        {
            using (var context = _contextFactory.CreateContext())
            {
                Poll matchingPoll = context.Polls.Where(s => s.ManageId == manageId).FirstOrDefault();
                if (matchingPoll == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("Poll {0} not found", manageId));
                }

                List<Vote> votesInManagedPoll = context.Votes.Where(v => v.PollId == matchingPoll.UUID).ToList();
                foreach (Vote vote in votesInManagedPoll)
                {
                    context.Votes.Remove(vote);
                }

                context.SaveChanges();

                return this.Request.CreateResponse(HttpStatusCode.OK);
            }
        }

        public HttpResponseMessage Delete(Guid manageId, long voteId)
        {
            using (var context = _contextFactory.CreateContext())
            {
                Poll matchingPoll = context.Polls.Where(s => s.ManageId == manageId).FirstOrDefault();
                if (matchingPoll == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("Poll {0} not found", manageId));
                }

                Vote matchingVote = context.Votes.Where(v => v.Id == voteId && v.PollId == matchingPoll.UUID).FirstOrDefault();

                context.Votes.Remove(matchingVote);
                context.SaveChanges();

                return this.Request.CreateResponse(HttpStatusCode.OK);
            }
        }

        #endregion
    }
}