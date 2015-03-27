﻿using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Http;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Controllers.API_Controllers;
using VotingApplication.Web.Api.Models.DBViewModels;

namespace VotingApplication.Web.Api.Controllers
{
    public class DashboardController : WebApiController
    {
        public DashboardController()
        {
        }

        public DashboardController(IContextFactory contextFactory)
            : base(contextFactory)
        {
        }

        [HttpGet]
        [Authorize]
        public List<DashboardPollResponseModel> Polls()
        {
            using (IVotingContext context = _contextFactory.CreateContext())
            {
                string userId = User.Identity.GetUserId();

                IEnumerable<DashboardPollResponseModel> userPolls = context
                    .Polls
                    .Where(p => p.CreatorIdentity == userId)
                    .OrderByDescending(p => p.CreatedDate)
                    .Select(CreateDashboardResponseFromModel);

                return userPolls.ToList();
            }
        }

        private static DashboardPollResponseModel CreateDashboardResponseFromModel(Poll poll)
        {
            return new DashboardPollResponseModel()
            {
                UUID = poll.UUID,
                ManageId = poll.ManageId,
                Name = poll.Name,
                Creator = poll.Creator,
                CreatedDate = poll.CreatedDate,
                ExpiryDate = poll.ExpiryDate
            };
        }


        [HttpPost]
        [Authorize]
        public CopyPollResponseModel Copy(CopyPollRequestModel pollCopyRequest)
        {
            if (pollCopyRequest == null)
            {
                this.ThrowError(HttpStatusCode.BadRequest);
            }

            if (!ModelState.IsValid)
            {
                this.ThrowError(HttpStatusCode.BadRequest, ModelState);
            }

            using (IVotingContext context = _contextFactory.CreateContext())
            {
                string userId = User.Identity.GetUserId();

                Poll pollToCopy = context
                    .Polls
                    .Include(p => p.Options)
                    .SingleOrDefault(p => p.UUID == pollCopyRequest.UUIDToCopy);

                if (pollToCopy == null)
                {
                    this.ThrowError(HttpStatusCode.BadRequest);
                }


                if (pollToCopy.CreatorIdentity != userId)
                {
                    this.ThrowError(HttpStatusCode.Forbidden);
                }

                Poll newPoll = CopyPoll(pollToCopy);

                context.Polls.Add(newPoll);
                context.SaveChanges();

                return new CopyPollResponseModel()
                {
                    newPollId = newPoll.UUID,
                    newManageId = newPoll.ManageId
                };
            }
        }

        private static Poll CopyPoll(Poll pollToCopy)
        {
            return new Poll()
            {
                UUID = Guid.NewGuid(),
                ManageId = Guid.NewGuid(),
                Name = "Copy of " + pollToCopy.Name,

                Creator = pollToCopy.Creator,
                CreatorIdentity = pollToCopy.CreatorIdentity,

                PollType = pollToCopy.PollType,

                MaxPoints = pollToCopy.MaxPoints,
                MaxPerVote = pollToCopy.MaxPerVote,

                InviteOnly = pollToCopy.InviteOnly,
                NamedVoting = pollToCopy.NamedVoting,
                OptionAdding = pollToCopy.OptionAdding,

                ExpiryDate = pollToCopy.ExpiryDate,
                LastUpdated = DateTime.Now,
                CreatedDate = DateTime.Now,


                Options = CopyOptions(pollToCopy.Options)
            };
        }

        private static List<Option> CopyOptions(IEnumerable<Option> options)
        {
            if (options == null)
            {
                return new List<Option>(0);
            }

            return options
                .Select(o => new Option()
                {
                    Name = o.Name,
                    Description = o.Description,
                    Info = o.Info
                })
                .ToList();
        }
    }
}