using System;

namespace VotingApplication.Web.Api.Models.DBViewModels
{
    public class ManagePollExpiryRequest
    {
        public DateTimeOffset? ExpiryDate { get; set; }
    }
}