using System;
using VotingApplication.Data.Model;

namespace VotingApplication.Web.Api.Services
{
    public interface ICorrespondenceService
    {
        void SendInvitation(Guid UUID, Ballot ballot, string pollQuestion);
        void SendConfirmation(string email, string authenticationToken);
    }
}
