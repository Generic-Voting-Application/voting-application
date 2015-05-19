using System;

namespace VotingApplication.Web.Api.Models.DBViewModels
{
    public class ManagePollExpiryRequest
    {
        public DateTime? ExpiryDate { get; set; }
    }
}