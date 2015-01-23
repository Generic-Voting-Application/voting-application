using System;

namespace VotingApplication.Data.Model
{
    public class ChatMessage
    {
        public long Id { get; set; }
        public String VoterName { get; set; }
        public String Message { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }
}
