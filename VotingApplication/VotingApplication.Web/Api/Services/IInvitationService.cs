using System;
using VotingApplication.Data.Model;

namespace VotingApplication.Web.Api.Services
{
    public interface IInvitationService
    {
        void SendInvitation(Guid UUID, Ballot ballot, string pollQuestion);
    }
}
