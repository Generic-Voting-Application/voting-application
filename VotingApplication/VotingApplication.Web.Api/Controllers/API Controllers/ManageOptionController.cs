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
    public class ManageOptionController : WebApiController
    {
        public ManageOptionController() : base() {}
        public ManageOptionController(IContextFactory contextFactory) : base(contextFactory) { }

        #region GET

        public virtual HttpResponseMessage Get(Guid manageId)
        {
            #region DB Get / Validation

            Poll poll;
            using (var context = _contextFactory.CreateContext())
            {
                poll = context.Polls.Where(s => s.ManageId == manageId).Include(s => s.Options).SingleOrDefault();
            }

            if (poll == null)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("Poll {0} not found", manageId));
            }

            #endregion

            #region Response

            List<OptionRequestResponseModel> response = new List<OptionRequestResponseModel>();

            foreach (Option option in poll.Options)
            {
                OptionRequestResponseModel responseOption = new OptionRequestResponseModel();

                responseOption.Name = option.Name;
                responseOption.Info = option.Info;
                responseOption.Description = option.Description;

                response.Add(responseOption);
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

        public virtual HttpResponseMessage Post(Guid manageId, OptionCreationRequestModel optionCreationRequest)
        {
            #region Input Validation

            if (optionCreationRequest == null)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            using (var context = _contextFactory.CreateContext())
            {
                Poll poll = context.Polls.Where(p => p.ManageId == manageId).FirstOrDefault();
                if (poll == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("Poll {0} does not exist", manageId));
                }
            }

            if (!ModelState.IsValid)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, ModelState);
            }

            #endregion

            #region DB Object Creation

            Option newOption = new Option();

            newOption.Name = optionCreationRequest.Name;
            newOption.Info = optionCreationRequest.Info;
            newOption.Description = optionCreationRequest.Description;

            using (var context = _contextFactory.CreateContext())
            {
                Poll poll = context.Polls.Where(p => p.ManageId == manageId).Single();
                if (poll.Options == null)
                {
                    poll.Options = new List<Option>();
                }

                poll.Options.Add(newOption);
                context.SaveChanges();
            }

            #endregion

            #region Response

            return this.Request.CreateResponse(HttpStatusCode.OK);

            #endregion
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
                Poll matchingPoll = context.Polls.Where(s => s.ManageId == manageId).Include(s => s.Options).FirstOrDefault();
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