using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Models.DBViewModels;
using VotingApplication.Web.Api.Controllers;

namespace VotingApplication.Web.Api.Controllers.API_Controllers
{
    public class ManageController : WebApiController
    {
        public ManageController() : base() { }
        public ManageController(IContextFactory contextFactory) : base(contextFactory) { }

        private ManagePollRequestResponseModel PollToModel(Poll poll)
        {
            return new ManagePollRequestResponseModel
            {
                UUID = poll.UUID,
                Options = poll.Options,
                VotingStrategy = poll.PollType.ToString(),
                MaxPoints = poll.MaxPoints,
                MaxPerVote = poll.MaxPerVote,
                InviteOnly = poll.InviteOnly,
                Name = poll.Name,
                AnonymousVoting = poll.AnonymousVoting,
                RequireAuth = poll.RequireAuth,
                Expires = poll.Expires,
                ExpiryDate = poll.ExpiryDate,
                OptionAdding = poll.OptionAdding
            };
        }

        #region GET

        public ManagePollRequestResponseModel Get(Guid manageId)
        {
            using (var context = _contextFactory.CreateContext())
            {
                Poll poll = context.Polls.Where(p => p.ManageId == manageId).Include(s => s.Options).FirstOrDefault();

                if (poll == null)
                {
                    this.ThrowError(HttpStatusCode.NotFound, string.Format("Poll for manage id {0} not found", manageId));
                }

                return PollToModel(poll);
            }
        }

        #endregion

        #region Put

        public void Put(Guid manageId, ManagePollUpdateRequest updateRequest)
        {

            #region Input Validation

            if (updateRequest == null)
            {
                this.ThrowError(HttpStatusCode.BadRequest);
            }

            if (updateRequest.Expires && updateRequest.ExpiryDate < DateTime.Now)
            {
                ModelState.AddModelError("ExpiryDate", "Invalid or unspecified ExpiryDate");
            }

            if (!ModelState.IsValid)
            {
                this.ThrowError(HttpStatusCode.BadRequest, ModelState);
            }

            #endregion

            using (var context = _contextFactory.CreateContext())
            {
                Poll existingPoll = context.Polls.Where(p => p.ManageId == manageId).SingleOrDefault();

                if (existingPoll == null)
                {
                    this.ThrowError(HttpStatusCode.NotFound, string.Format("Poll for manage id {0} not found", manageId));
                }

                existingPoll.AnonymousVoting = updateRequest.AnonymousVoting;
                existingPoll.Expires = updateRequest.Expires;
                existingPoll.ExpiryDate = updateRequest.ExpiryDate;
                existingPoll.InviteOnly = updateRequest.InviteOnly;
                existingPoll.MaxPerVote = updateRequest.MaxPerVote;
                existingPoll.MaxPoints = updateRequest.MaxPoints;
                existingPoll.Name = updateRequest.Name;
                existingPoll.OptionAdding = updateRequest.OptionAdding;
                existingPoll.RequireAuth = updateRequest.RequireAuth;

                existingPoll.LastUpdated = DateTime.Now;

                // Need code to handle poll type changed when enabaled.

                context.SaveChanges();
            }
        }

        #endregion
    }
}