using System;

namespace VotingApplication.Data.Model
{
    public class Token
    {
        public long Id { get; set; }
        public Guid TokenGuid { get; set; }

        public long UserId { get; set; }
        public Guid PollId { get; set; }
    }
}
