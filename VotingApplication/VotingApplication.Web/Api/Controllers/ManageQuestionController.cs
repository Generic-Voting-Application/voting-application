using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Metrics;
using VotingApplication.Web.Api.Models.DBViewModels;

namespace VotingApplication.Web.Api.Controllers
{
    public class ManageQuestionController : WebApiController
    {
        public ManageQuestionController() : base() { }

        public ManageQuestionController(IContextFactory contextFactory, IMetricEventHandler metricHandler) : base(contextFactory, metricHandler) { }

        [HttpPut]
        public void Put(Guid manageId, ManageQuestionRequest request)
        {
            ValidateRequest(request);

            if (!ModelState.IsValid)
            {
                ThrowError(HttpStatusCode.BadRequest, ModelState);
            }

            using (var context = _contextFactory.CreateContext())
            {
                Poll poll = PollByManageId(manageId, context);

                if (String.IsNullOrWhiteSpace(request.Question))
                {
                    ThrowError(HttpStatusCode.BadRequest, "Question cannot be null or empty");
                }

                if (poll.Name == request.Question)
                {
                    return;
                }

                _metricHandler.QuestionChangedEvent(request.Question, poll.UUID);

                poll.Name = request.Question;
                poll.LastUpdated = DateTime.Now;

                context.SaveChanges();
            }
        }

        private void ValidateRequest(ManageQuestionRequest request)
        {
            if (request == null)
            {
                ThrowError(HttpStatusCode.BadRequest, "Question request cannot be null");
            }
        }
    }
}
