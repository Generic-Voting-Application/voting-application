using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VotingApplication.Web.Api.Models.DBViewModels
{
    public class ManageInvitationRequestModel
    {
        public Boolean SendInvitation { get; set; }
        public string Email { get; set; }
        public Guid ManageToken { get; set; }
    }
}