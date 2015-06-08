
namespace VotingApplication.Data.Model
{
    /*
     * < 0: Errors
     * 1-99: Navigation
     * 100-199: Poll Configuration
     * 200-299: Option management
     * 300-399: Vote management
     * 400-499: Ballot management
     * 500-599: Poll creation
     * 1000+: Accounts
     */
    public enum MetricType
    {
        Error = -1,
        GoToPage = 1,
        UpdateResults = 2,
        SetExpiry = 100,
        SetPollType = 101,
        SetQuestion = 102,
        SetInviteOnly = 103,
        SetNamedVoting = 104,
        SetOptionAdding = 105,
        SetIsElectionMode = 106,
        AddOption = 200,
        UpdateOption = 201,
        DeleteOption = 202,
        AddVote = 300,
        DeleteVote = 301,
        AddBallot = 400,
        DeleteBallot = 401,
        CreatePoll = 500,
        ClonePoll = 501,
        Login = 1000,
        Logout = 1001,
        Register = 1002
    }
}
