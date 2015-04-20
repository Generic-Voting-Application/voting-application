using System;
using System.ComponentModel.DataAnnotations;

namespace VotingApplication.Data.Model
{
    public class Event
    {
        public Event(string eventType, Guid pollId)
        {
            Timestamp = DateTime.Now;
            EventType = eventType;
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
