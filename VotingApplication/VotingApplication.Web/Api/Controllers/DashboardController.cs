using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Models.DBViewModels;

namespace VotingApplication.Web.Api.Controllers
{
    public class DashboardController : WebApiController
    {
        public DashboardController()
        {
        }

        public DashboardController(IContextFactory contextFactory)
            : base(contextFactory)
        {
        }

        [HttpGet]
        [Authorize]
        public List<DashboardPollResponseModel> Polls()
        {
            using (IVotingContext context = _contextFactory.CreateContext())
            {
                string userId = User.Identity.GetUserId();

                IEnumerable<DashboardPollResponseModel> userPolls = context
                    .Polls
                    .Where(p => p.CreatorIdentity == userId)
                    .OrderByDescending(p => p.CreatedDate)
                    .Select(CreateDashboardResponseFromModel);

                return userPolls.ToList();
            }
        }

        private static DashboardPollResponseModel CreateDashboardResponseFromModel(Poll poll)
        {
            return new DashboardPollResponseModel()
            {
                UUID = poll.UUID,
                ManageId = poll.ManageId,
                Name = poll.Name,
                Creator = poll.Creator,
                CreatedDate = poll.CreatedDate,
                ExpiryDate = poll.ExpiryDate
            };
        }


        // POST: api/Dashboard
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Dashboard/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Dashboard/5
        public void Delete(int id)
        {
        }
    }
}
