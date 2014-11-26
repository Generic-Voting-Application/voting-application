﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VotingApplication.Data.Model
{
    public class Session
    {
        public long Id { get; set; }

        public Guid UUID { get; set; }
        public Guid ManageID { get; set; }
        public string Name { get; set; }

        public string Creator { get; set; }

        public string VotingStrategy { get; set; }

        [NotMapped]
        public string Email { get; set; }
        [NotMapped]
        public List<string> Invites { get; set; }

        public long OptionSetId { get; set; }
        public List<Option> Options { get; set; }
    }
}
