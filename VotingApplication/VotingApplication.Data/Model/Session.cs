using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VotingApplication.Data.Model
{
    public class Session
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public long OptionSetId { get; set; }
        public OptionSet OptionSet { get; set; }
    }
}
