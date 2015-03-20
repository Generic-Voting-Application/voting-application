using System;

namespace VotingApplication.Web.Api.Models.DBViewModels
{
    public class CopyPollResponseModel
    {
        public Guid newPollId { get; set; }
        public Guid newManageId { get; set; }
    }
}