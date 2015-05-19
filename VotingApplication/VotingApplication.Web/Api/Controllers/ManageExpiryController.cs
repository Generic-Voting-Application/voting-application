using System;
using System.Net;
using System.Web.Http;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Metrics;
using VotingApplication.Web.Api.Models.DBViewModels;

namespace VotingApplication.Web.Api.Controllers
{
    public class ManageExpiryController : WebApiController
    {
        public ManageExpiryController() : base() { }

        public ManageExpiryController(IContextFactory contextFactory, IMetricHandler metricHandler) : base(contextFactory, metricHandler) { }

        [HttpPut]
        public void Put(Guid manageId, ManagePollExpiryRequest updateRequest)
        {
            using (var context = _contextFactory.CreateContext())
            {
                Poll poll = PollByManageId(manageId, context);

                if (updateRequest.ExpiryDateUtc.HasValue && updateRequest.ExpiryDateUtc < DateTime.UtcNow)
                {
                    ModelState.AddModelError("ExpiryDateUtc", "Invalid ExpiryDateUtc");
                }

                if (!ModelState.IsValid)
                {
                    ThrowError(HttpStatusCode.BadRequest, ModelState);
                }

                if (poll.ExpiryDateUtc == updateRequest.ExpiryDateUtc)
                {
                    return;
                }

                poll.ExpiryDateUtc = updateRequest.ExpiryDateUtc;
                poll.LastUpdatedUtc = DateTime.UtcNow;
                _metricHandler.HandleExpiryChangedEvent(poll.ExpiryDateUtc, poll.UUID);

                context.SaveChanges();
            }
        }
    }
}
