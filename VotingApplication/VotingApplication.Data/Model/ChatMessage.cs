using System;

namespace VotingApplication.Data.Model
{
    public class ChatMessage
    {
        public long Id { get; set; }
        public User User { get; set; }
        public String Message { get; set; }
    }
}
