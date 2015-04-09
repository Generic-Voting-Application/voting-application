using System;
using System.ComponentModel.DataAnnotations;

namespace VotingApplication.Web.Api.Models.DBViewModels
{
    public class ManagePollExpiryRequest
    {
        [Required]
        public DateTime? ExpiryDate { get; set; }
    }
}