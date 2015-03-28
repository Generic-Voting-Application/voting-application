using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VotingApplication.Web.Api.Models.DBViewModels
{
    public class TokenRequestModel
    {
        public Boolean EmailSent { get; set; }
        public string Email { get; set; }
    }
}