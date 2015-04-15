using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Models.DBViewModels;

namespace VotingApplication.Web.Api.Controllers
{
    public class ManageExpiryController : WebApiController
    {
        public ManageExpiryController() : base() { }

        public ManageExpiryController(IContextFactory contextFactory) : base(contextFactory) { }

        [HttpPut]
        public void Put(Guid manageId, ManagePollExpiryRequest updateRequest)
        {
            using (var context = _contextFactory.CreateContext())
            {
                Poll poll = context.Polls.Where(p => p.ManageId == manageId).SingleOrDefault();

                if (poll == null)
                {
                    ThrowError(HttpStatusCode.NotFound, string.Format("Poll for manage id {0} not found", manageId));
                }

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

                context.SaveChanges();
            }
        }
    }
}
