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

        public List<OptionRequestResponseModel> Get(Guid manageId)
        {
            using (var context = _contextFactory.CreateContext())
            {
                Poll poll = context.Polls.Where(s => s.ManageId == manageId).Include(s => s.Options).SingleOrDefault();

                if (poll == null)
                {
                    this.ThrowError(HttpStatusCode.NotFound, string.Format("Poll {0} not found", manageId));
                }

                return poll.Options.Select(OptionToModel).ToList();
            }
        }

        #endregion

        #region POST

        public void Post(Guid manageId, OptionCreationRequestModel optionCreationRequest)
        {
            if (optionCreationRequest == null)
            {
                this.ThrowError(HttpStatusCode.BadRequest);
            }

            using (var context = _contextFactory.CreateContext())
            {
                Poll poll = context.Polls.Where(p => p.ManageId == manageId).FirstOrDefault();
                if (poll == null)
                {
                    this.ThrowError(HttpStatusCode.NotFound, string.Format("Poll {0} does not exist", manageId));
                }
            }

            if (!ModelState.IsValid)
            {
                this.ThrowError(HttpStatusCode.BadRequest, ModelState);
            }

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

            return;
        }

        #endregion

        #region PUT

        public void Put(Guid manageId, List<Option> options)
        {
            using (var context = _contextFactory.CreateContext())
            {
                Poll matchingPoll = context.Polls.Where(p => p.ManageId == manageId).Include(p => p.Options).SingleOrDefault();
                if (matchingPoll == null)
                {
                    this.ThrowError(HttpStatusCode.NotFound, string.Format("Poll {0} not found", manageId));
                }

                // Check validity of options
                if (options.Any(p => String.IsNullOrEmpty(p.Name)))
                {
                    this.ThrowError(HttpStatusCode.BadRequest, string.Format("Option name must not be empty"));
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

                return;
            }
        }

        #endregion

        #region DELETE

        public void Delete(Guid manageId, long optionId)
        {
            using (var context = _contextFactory.CreateContext())
            {
                Poll matchingPoll = context.Polls.Where(s => s.ManageId == manageId).Include(s => s.Options).FirstOrDefault();
                if (matchingPoll == null)
                {
                    this.ThrowError(HttpStatusCode.NotFound, string.Format("Poll {0} not found", manageId));
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

                return;
            }
        }

        #endregion
    }
}