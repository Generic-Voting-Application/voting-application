using System;

namespace VotingApplication.Data.Model
{
    public class Token
    {
        public long Id { get; set; }
        public Guid TokenGuid { get; set; }
        public string Email { get; set; }
    }
}
