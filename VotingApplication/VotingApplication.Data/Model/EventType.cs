using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VotingApplication.Data.Model
{
    public enum EventType
    {
        Error,
        GoToPage,
        CreatePoll,
        UpdateResults,
        SetExpiry,
        SetPollType,
        SetQuestion,
        SetInviteOnly,
        SetNamedVoting,
        SetOptionAdding,
        SetHiddenResults,
        AddOption,
        UpdateOption,
        DeleteOption,
        AddVote,
        DeleteVote,
        AddBallot,
        DeleteBallot,
        Login,
        Logout,
        Register
    }
}
