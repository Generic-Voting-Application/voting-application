using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Models.DBViewModels;

namespace VotingApplication.Web.Api.Controllers
{
    public class ManageQuestionController : WebApiController
    {
        public ManageQuestionController() : base() { }

        public ManageQuestionController(IContextFactory contextFactory) : base(contextFactory) { }

        [HttpPut]
        public void Put(Guid manageId, ManageQuestionRequest request)
        {
            ValidateRequest(request);

            using (var context = _contextFactory.CreateContext())
            {
                Poll poll = GetPoll(context, manageId);

                if (String.IsNullOrWhiteSpace(request.Question))
                {
                    ThrowError(HttpStatusCode.BadRequest, "Question cannot be null or empty");
                }

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

        private Poll GetPoll(IVotingContext context, Guid manageId)
        {
            Poll matchingPoll = context.Polls
                                        .Where(p => p.ManageId == manageId)
                                        .FirstOrDefault();

            if (matchingPoll == null)
            {
                ThrowError(HttpStatusCode.NotFound, string.Format("Poll {0} not found", manageId));
            }

            return matchingPoll;
        }
    }
}
