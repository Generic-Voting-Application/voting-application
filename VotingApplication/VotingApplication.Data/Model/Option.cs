using System;
using System.ComponentModel.DataAnnotations;

namespace VotingApplication.Data.Model
{
    public class Option
    {
        public long Id { get; set; }

        public string Name { get; set; }
    }
}
