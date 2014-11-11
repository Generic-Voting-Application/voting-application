using System.Collections.Generic;

namespace VotingApplication.Data.Model
{
    public class OptionSet
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public List<Option> Options { get; set; }
    }
}
