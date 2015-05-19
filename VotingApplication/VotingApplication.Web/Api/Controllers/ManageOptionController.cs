using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Http;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Metrics;
using VotingApplication.Web.Api.Models.DBViewModels;

namespace VotingApplication.Web.Api.Controllers
{
    public class ManageOptionController : WebApiController
    {
        public ManageOptionController()
        {
        }

        public ManageOptionController(IContextFactory contextFactory, IMetricHandler metricHandler) : base(contextFactory, metricHandler) { }

        [HttpGet]
        public List<ManageOptionResponseModel> Get(Guid manageId)
        {
            using (IVotingContext context = _contextFactory.CreateContext())
            {
                Poll poll = PollByManageId(manageId, context);

                return poll
                    .Options
                    .Select(CreateOptionResponseModel)
                    .ToList();
            }
        }

        public ManageOptionResponseModel CreateOptionResponseModel(Option option)
        {
            return new ManageOptionResponseModel()
            {
                OptionNumber = option.PollOptionNumber,
                Name = option.Name,
                Description = option.Description
            };
        }

        [HttpPut]
        public void Put(Guid manageId, ManageOptionUpdateRequest request)
        {
            ValidateRequest(request);

            if (!ModelState.IsValid)
            {
                ThrowError(HttpStatusCode.BadRequest, ModelState);
            }

            using (IVotingContext context = _contextFactory.CreateContext())
            {
                Poll poll = GetPoll(manageId, context);

                List<int> existingPollOptionNumbers = GetExistingPollOptionNumbers(poll);
                List<int> requestPollOptionNumbers = GetRequestPollOptionNumbers(request);

                if (requestPollOptionNumbers.Except(existingPollOptionNumbers).Any())
                {
                    ThrowError(HttpStatusCode.NotFound, String.Format("Options do not all belong to poll {0}", manageId));
                }


                List<int> optionsToRemove = existingPollOptionNumbers
                    .Except(requestPollOptionNumbers)
                    .ToList();

                RemoveOptions(context, poll, optionsToRemove);


                List<int> optionsToUpdate = existingPollOptionNumbers
                    .Intersect(requestPollOptionNumbers)
                    .ToList();

                UpdateOptions(request, poll, optionsToUpdate);


                List<OptionUpdate> optionUpdatesToAdd = request
                    .Options
                    .Where(o => o.OptionNumber.HasValue == false)
                    .ToList();

                AddNewOptions(context, poll, optionUpdatesToAdd);


                poll.LastUpdatedUtc = DateTime.Now;
                context.SaveChanges();
            }
        }

        private Poll GetPoll(Guid manageId, IVotingContext context)
        {
            Poll poll = context
                .Polls
                .Include(p => p.Options)
                .SingleOrDefault(p => p.ManageId == manageId);

            if (poll == null)
            {
                ThrowError(HttpStatusCode.NotFound, string.Format("Poll for manage id {0} not found", manageId));
            }
            return poll;
        }

        private void ValidateRequest(ManageOptionUpdateRequest request)
        {
            if (request == null)
            {
                ThrowError(HttpStatusCode.BadRequest);
            }
        }

        private static List<int> GetExistingPollOptionNumbers(Poll poll)
        {
            List<int> existingPollOptionNumbers = poll
                .Options
                .Select(o => o.PollOptionNumber)
                .ToList();
            return existingPollOptionNumbers;
        }

        private static List<int> GetRequestPollOptionNumbers(ManageOptionUpdateRequest request)
        {
            List<int> requestPollOptionNumbers = request
                .Options
                .Where(o => o.OptionNumber.HasValue)
                .Select(o => o.OptionNumber.Value)
                .ToList();
            return requestPollOptionNumbers;
        }

        private void RemoveOptions(IVotingContext context, Poll poll, IEnumerable<int> optionsToRemove)
        {
            foreach (int pollOptionNumber in optionsToRemove)
            {
                Option option = poll
                    .Options
                    .Single(o => o.PollOptionNumber == pollOptionNumber);

                _metricHandler.HandleOptionDeletedEvent(option, poll.UUID);
                poll.Options.Remove(option);
                context.Options.Remove(option);

                List<Vote> votes = context
                    .Votes
                    .Include(v => v.Option)
                    .Where(v => v.Option.PollOptionNumber == pollOptionNumber)
                    .ToList();

                foreach (Vote vote in votes)
                {
                    Ballot ballot = context
                        .Ballots
                        .Include(b => b.Votes)
                        .Single(b => b.Votes.Any(v => v.Id == vote.Id));

                    _metricHandler.HandleVoteDeletedEvent(vote, poll.UUID);
                    ballot.Votes.Remove(vote);
                    context.Votes.Remove(vote);
                }
            }
        }

        private void UpdateOptions(ManageOptionUpdateRequest request, Poll poll, IEnumerable<int> optionsToUpdate)
        {
            foreach (int pollOptionNumber in optionsToUpdate)
            {
                Option option = poll
                    .Options
                    .Single(o => o.PollOptionNumber == pollOptionNumber);

                OptionUpdate update = request
                    .Options
                    .Single(o => o.OptionNumber == pollOptionNumber);

                option.Name = update.Name;
                option.Description = update.Description;

                _metricHandler.HandleOptionUpdatedEvent(option, poll.UUID);
            }
        }

        private void AddNewOptions(IVotingContext context, Poll poll, IEnumerable<OptionUpdate> optionUpdatesToAdd)
        {
            foreach (OptionUpdate optionRequest in optionUpdatesToAdd)
            {
                var option = new Option
                {
                    Name = optionRequest.Name,
                    Description = optionRequest.Description
                };

                _metricHandler.HandleOptionAddedEvent(option, poll.UUID);

                poll.Options.Add(option);
                context.Options.Add(option);
            }
        }
    }
}
