using System;
using VotingApplication.Data.Model;

namespace VotingApplication.Web.Api.Models.DBViewModels
{
    public class PollCreationResponseModel
    {
        public Guid UUID { get; set; }
        public Guid ManageId { get; set; }
        public Ballot CreatorBallot { get; set; }
    }
}