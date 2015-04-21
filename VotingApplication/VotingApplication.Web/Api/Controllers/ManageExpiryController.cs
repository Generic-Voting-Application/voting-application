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
    public class ManageExpiryController : WebApiController
    {
        public ManageExpiryController() : base() { }

        public ManageExpiryController(IContextFactory contextFactory, IMetricEventHandler metricHandler) : base(contextFactory, metricHandler) { }

        [HttpPut]
        public void Put(Guid manageId, ManagePollExpiryRequest updateRequest)
        {
            using (var context = _contextFactory.CreateContext())
            {
                Poll poll = PollByManageId(manageId);

                if (updateRequest.ExpiryDate.HasValue && updateRequest.ExpiryDate < DateTime.Now)
                {
                    ModelState.AddModelError("ExpiryDate", "Invalid ExpiryDate");
                }

                if (!ModelState.IsValid)
                {
                    ThrowError(HttpStatusCode.BadRequest, ModelState);
                }

                poll.ExpiryDate = updateRequest.ExpiryDate;
                poll.LastUpdated = DateTime.Now;
                _metricHandler.SetExpiry(poll.ExpiryDate, poll.UUID);

                context.SaveChanges();
            }
        }
    }
}
