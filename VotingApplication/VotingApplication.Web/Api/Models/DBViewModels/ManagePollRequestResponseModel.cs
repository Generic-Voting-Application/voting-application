using System;
using System.Collections.Generic;
using VotingApplication.Data.Model;

namespace VotingApplication.Web.Api.Models.DBViewModels
{
    public class ManagePollRequestResponseModel
    {
        public Guid UUID { get; set; }
        public List<Option> Options { get; set; }
    }
}