using System;

namespace VotingApplication.Web.Api.Models.DBViewModels
{
    public class DashboardPollResponseModel
    {
        public Guid UUID { get; set; }
        public Guid ManageId { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDateUtc { get; set; }
    }
}