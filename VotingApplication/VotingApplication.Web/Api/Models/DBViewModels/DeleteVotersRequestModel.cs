using System;
using System.Collections.Generic;

namespace VotingApplication.Web.Api.Models.DBViewModels
{
    public class DeleteVotersRequestModel
    {
        public DeleteVotersRequestModel()
        {
            BallotDeleteRequests = new List<DeleteBallotRequestModel>();
        }

        public List<DeleteBallotRequestModel> BallotDeleteRequests { get; set; }
    }

    public class DeleteBallotRequestModel
    {
        public DeleteBallotRequestModel()
        {
            VoteDeleteRequests = new List<DeleteVoteRequestModel>();
        }

        public Guid BallotManageGuid { get; set; }
        public List<DeleteVoteRequestModel> VoteDeleteRequests { get; set; }
    }

    public class DeleteVoteRequestModel
    {
        public int ChoiceNumber { get; set; }
    }
}