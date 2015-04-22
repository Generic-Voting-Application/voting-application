using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Models.DBViewModels;

namespace VotingApplication.Web.Api.Controllers
{
    public class ManageMiscController : WebApiController
    {
        public ManageMiscController() : base() { }

        public ManageMiscController(IContextFactory contextFactory) : base(contextFactory) { }

        [HttpPut]
        public void Put(Guid manageId, ManagePollMiscRequest updateRequest)
        {
            using (var context = _contextFactory.CreateContext())
            {
                Poll poll = context
                    .Polls
                    .SingleOrDefault(p => p.ManageId == manageId);

                if (poll == null)
                {
                    ThrowError(HttpStatusCode.NotFound, string.Format("Poll for manage id {0} not found", manageId));
                }

                if (!ModelState.IsValid)
                {
                    ThrowError(HttpStatusCode.BadRequest, ModelState);
                }

                poll.InviteOnly = updateRequest.InviteOnly;
                poll.NamedVoting = updateRequest.NamedVoting;
                poll.OptionAdding = updateRequest.OptionAdding;
                poll.HiddenResults = updateRequest.HiddenResults;

                poll.LastUpdated = DateTime.Now;

                context.SaveChanges();
            }
        }
    }
}
