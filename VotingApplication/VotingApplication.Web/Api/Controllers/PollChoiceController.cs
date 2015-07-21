using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Http;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Metrics;
using VotingApplication.Web.Api.Models.DBViewModels;
using VotingApplication.Web.Api.SignalR;

namespace VotingApplication.Web.Api.Controllers
{
    public class PollChoiceController : WebApiController
    {
        public PollChoiceController() : base() { }

        public PollChoiceController(IContextFactory contextFactory, IMetricHandler metricHandler) : base(contextFactory, metricHandler) { }

        [HttpPost]
        public void Post(Guid pollId, ChoiceCreationRequestModel choiceCreationRequest)
        {
            using (IVotingContext context = _contextFactory.CreateContext())
            {
                if (choiceCreationRequest == null)
                {
                    ThrowError(HttpStatusCode.BadRequest);
                }

                Poll poll = context
                    .Polls
                    .Include(p => p.Choices)
                    .SingleOrDefault(p => p.UUID == pollId);

                if (poll == null)
                {
                    ThrowError(HttpStatusCode.NotFound, string.Format("Poll {0} does not exist", pollId));
                }

                if (!poll.ChoiceAdding)
                {
                    ThrowError(HttpStatusCode.MethodNotAllowed, string.Format("Option adding not allowed for poll {0}", pollId));
                }

                if (!ModelState.IsValid)
                {
                    ThrowError(HttpStatusCode.BadRequest, ModelState);
                }

                Choice newOption = CreateChoiceFromRequest(choiceCreationRequest);

                _metricHandler.HandleChoiceAddedEvent(newOption, pollId);

                poll.Choices.Add(newOption);
                context.Choices.Add(newOption);

                poll.LastUpdatedUtc = DateTime.UtcNow;

                context.SaveChanges();

                ClientSignaller.SignalUpdate(poll.UUID.ToString());
            }
        }

        private static Choice CreateChoiceFromRequest(ChoiceCreationRequestModel requestModel)
        {
            return new Choice
            {
                Name = requestModel.Name,
                Description = requestModel.Description
            };
        }
    }
}