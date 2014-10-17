using System;
using System.ComponentModel.DataAnnotations;

namespace VotingApplication.Data.Model
{
    public class User
    {
        [Key]
        public virtual long Id { get; set; }
        public virtual string Name { get; set; }
    }
}
