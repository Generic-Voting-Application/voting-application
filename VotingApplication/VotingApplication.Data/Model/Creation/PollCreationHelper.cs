
using System;
using System.Collections.Generic;

namespace VotingApplication.Data.Model.Creation
{
    public static class PollCreationHelper
    {
        public static Poll Create()
        {
            return new Poll()
            {
                UUID = Guid.NewGuid(),
                ManageId = Guid.NewGuid(),

                PollType = PollType.Basic,
                Options = new List<Option>(),
                MaxPoints = 7,
                MaxPerVote = 3,
                InviteOnly = false,
                NamedVoting = false,
                RequireAuth = false,
                Expires = false,
                ExpiryDate = null,
                OptionAdding = false,

                CreatedDate = DateTime.Now,
                LastUpdated = DateTime.Now
            };
        }

    }
}
