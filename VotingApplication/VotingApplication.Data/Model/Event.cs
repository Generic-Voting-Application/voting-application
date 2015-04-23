using System;
using System.ComponentModel.DataAnnotations;

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

    public class Event
    {
        public Event(EventType eventType, Guid pollId)
        {
            Timestamp = DateTime.Now;
            EventType = eventType.ToString();
            PollId = pollId;
        }

        public long Id { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }

        [Required]
        public string EventType { get; set; }

        [Required]
        public Guid PollId { get; set; }

        public string Value { get; set; }
        public string Detail { get; set; }
    }
}
