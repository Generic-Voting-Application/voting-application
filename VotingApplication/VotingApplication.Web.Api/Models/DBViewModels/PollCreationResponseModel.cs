using System;

namespace VotingApplication.Web.Api.Models.DBViewModels
{
    public class PollCreationResponseModel
    {
        public Guid UUID { get; set; }
        public Guid ManageId { get; set; }
    }
}