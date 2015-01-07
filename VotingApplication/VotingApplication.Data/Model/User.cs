using System;
using System.ComponentModel.DataAnnotations;

namespace VotingApplication.Data.Model
{
    public class User
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public Token Token { get; set; }

        public Guid PollId { get; set; }
    }
}
