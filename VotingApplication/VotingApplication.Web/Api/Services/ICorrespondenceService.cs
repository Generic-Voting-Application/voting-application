using System;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Models;

namespace VotingApplication.Web.Api.Services
{
    public interface ICorrespondenceService
    {
        void SendInvitation(Guid UUID, Ballot ballot, string pollQuestion);
        void SendConfirmation(ApplicationUser user, string code);
    }
}
