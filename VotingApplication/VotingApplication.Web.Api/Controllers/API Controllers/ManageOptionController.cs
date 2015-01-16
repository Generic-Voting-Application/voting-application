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
        public ManageOptionController() : base() { }
        public ManageOptionController(IContextFactory contextFactory) : base(contextFactory) { }

        private OptionRequestResponseModel OptionToModel(Option option)
        {
            return new OptionRequestResponseModel
            {
                Name = option.Name,
                Info = option.Info,
                Description = option.Description
            };
        }

        private Option ModelToOption(OptionCreationRequestModel model)
        {
            return new Option
            {
                Name = model.Name,
                Info = model.Info,
                Description = model.Description
            };
        }

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

            return this.Request.CreateResponse(HttpStatusCode.OK, poll.Options.Select(OptionToModel).ToList());
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

            Option newOption = ModelToOption(optionCreationRequest);

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

        public virtual HttpResponseMessage Post(Guid manageId, long voteId, OptionCreationRequestModel option)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use POST by id on this controller");
        }

        #endregion

        #region PUT

        public virtual HttpResponseMessage Put(Guid manageId, List<Option> options)
        {
            using (var context = _contextFactory.CreateContext())
            {
                Poll matchingPoll = context.Polls.Where(p => p.ManageId == manageId).Include(p => p.Options).SingleOrDefault();
                if (matchingPoll == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("Poll {0} not found", manageId));
                }

                // Check validity of options
                if (options.Any(p => String.IsNullOrEmpty(p.Name)))
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, string.Format("Option name must not be empty"));
                }

                // Remove all old options
                foreach (Option oldOption in matchingPoll.Options.ToList<Option>())
                {
                    context.Options.Remove(oldOption);
                }

                // Add all new options
                foreach (Option newOption in options)
                {
                    context.Options.Add(newOption);
                }

                matchingPoll.Options = options;
                context.SaveChanges();

                return this.Request.CreateResponse(HttpStatusCode.OK);
            }
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