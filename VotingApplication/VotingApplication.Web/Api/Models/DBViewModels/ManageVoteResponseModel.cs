using System;
using System.Collections.Generic;

namespace VotingApplication.Web.Api.Models.DBViewModels
{
    public class ManageVoteResponseModel
    {
        public Guid BallotManageGuid { get; set; }
        public string VoterName { get; set; }
        public List<VoteResponse> Votes { get; set; }

        public ManageVoteResponseModel()
        {
            VoterName = "Anonymous User";
        }
    }

    public class VoteResponse
    {
        public int ChoiceNumber { get; set; }
        public string ChoiceName { get; set; }
        public int Value { get; set; }
    }
}