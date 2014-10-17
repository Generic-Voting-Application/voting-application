using System;
using System.ComponentModel.DataAnnotations;

namespace VotingApplication.Data.Model
{
    public class Vote
    {
        [Key]
        public virtual long Id  { get; set; }
        public virtual Option Option { get; set; }
        public virtual User User { get; set; }
    }
}
