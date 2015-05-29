using System;

namespace VotingApplication.Web.Api.Models.DBViewModels
{
    public class CopyPollResponseModel
    {
        public Guid NewPollId { get; set; }
        public Guid NewManageId { get; set; }
        public Guid CreatorBallotToken { get; set; }
    }
}