using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VotingApplication.Data.Model
{
    public class Session
    {
        public long Id { get; set; }

        public Guid UUID { get; set; }
        public string Name { get; set; }

        public long OptionSetId { get; set; }
        public List<Option> Options { get; set; }
    }
}
