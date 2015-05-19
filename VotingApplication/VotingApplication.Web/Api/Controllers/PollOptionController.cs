using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Http;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Metrics;
using VotingApplication.Web.Api.Models.DBViewModels;

namespace VotingApplication.Web.Api.Controllers
{
    public class PollOptionController : WebApiController
    {
        public PollOptionController() : base() { }

        public PollOptionController(IContextFactory contextFactory, IMetricHandler metricHandler) : base(contextFactory, metricHandler) { }

        [HttpPost]
        public void Post(Guid pollId, OptionCreationRequestModel optionCreationRequest)
        {
            using (IVotingContext context = _contextFactory.CreateContext())
            {
                if (optionCreationRequest == null)
                {
                    ThrowError(HttpStatusCode.BadRequest);
                }

                Poll poll = context
                    .Polls
                    .Include(p => p.Options)
                    .SingleOrDefault(p => p.UUID == pollId);

                if (poll == null)
                {
                    ThrowError(HttpStatusCode.NotFound, string.Format("Poll {0} does not exist", pollId));
                }

                if (!poll.OptionAdding)
                {
                    ThrowError(HttpStatusCode.MethodNotAllowed, string.Format("Option adding not allowed for poll {0}", pollId));
                }

                if (!ModelState.IsValid)
                {
                    ThrowError(HttpStatusCode.BadRequest, ModelState);
                }

                Option newOption = CreateOptionFromRequest(optionCreationRequest);

                _metricHandler.HandleOptionAddedEvent(newOption, pollId);

                poll.Options.Add(newOption);
                context.Options.Add(newOption);

                poll.LastUpdatedUtc = DateTime.Now;

                context.SaveChanges();
            }
        }

        private static Option CreateOptionFromRequest(OptionCreationRequestModel requestModel)
        {
            return new Option
            {
                Name = requestModel.Name,
                Description = requestModel.Description
            };
        }
    }
}