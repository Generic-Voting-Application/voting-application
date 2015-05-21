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
    public class ManageChoiceController : WebApiController
    {
        public ManageChoiceController()
        {
        }

        public ManageChoiceController(IContextFactory contextFactory, IMetricHandler metricHandler) : base(contextFactory, metricHandler) { }

        [HttpGet]
        public List<ManageChoiceResponseModel> Get(Guid manageId)
        {
            using (IVotingContext context = _contextFactory.CreateContext())
            {
                Poll poll = PollByManageId(manageId, context);

                return poll
                    .Choices
                    .Select(CreateOptionResponseModel)
                    .ToList();
            }
        }

        public ManageChoiceResponseModel CreateOptionResponseModel(Choice option)
        {
            return new ManageChoiceResponseModel()
            {
                ChoiceNumber = option.PollChoiceNumber,
                Name = option.Name,
                Description = option.Description
            };
        }

        [HttpPut]
        public void Put(Guid manageId, ManageChoiceUpdateRequest request)
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


                List<ChoiceUpdate> optionUpdatesToAdd = request
                    .Choices
                    .Where(o => o.ChoiceNumber.HasValue == false)
                    .ToList();

                AddNewOptions(context, poll, optionUpdatesToAdd);


                poll.LastUpdatedUtc = DateTime.UtcNow;
                context.SaveChanges();
            }
        }

        private Poll GetPoll(Guid manageId, IVotingContext context)
        {
            Poll poll = context
                .Polls
                .Include(p => p.Choices)
                .SingleOrDefault(p => p.ManageId == manageId);

            if (poll == null)
            {
                ThrowError(HttpStatusCode.NotFound, string.Format("Poll for manage id {0} not found", manageId));
            }
            return poll;
        }

        private void ValidateRequest(ManageChoiceUpdateRequest request)
        {
            if (request == null)
            {
                ThrowError(HttpStatusCode.BadRequest);
            }
        }

        private static List<int> GetExistingPollOptionNumbers(Poll poll)
        {
            List<int> existingPollOptionNumbers = poll
                .Choices
                .Select(o => o.PollChoiceNumber)
                .ToList();
            return existingPollOptionNumbers;
        }

        private static List<int> GetRequestPollOptionNumbers(ManageChoiceUpdateRequest request)
        {
            List<int> requestPollOptionNumbers = request
                .Choices
                .Where(o => o.ChoiceNumber.HasValue)
                .Select(o => o.ChoiceNumber.Value)
                .ToList();
            return requestPollOptionNumbers;
        }

        private void RemoveOptions(IVotingContext context, Poll poll, IEnumerable<int> optionsToRemove)
        {
            foreach (int pollOptionNumber in optionsToRemove)
            {
                Choice option = poll
                    .Choices
                    .Single(o => o.PollChoiceNumber == pollOptionNumber);

                _metricHandler.HandleChoiceDeletedEvent(option, poll.UUID);
                poll.Choices.Remove(option);
                context.Choices.Remove(option);

                List<Vote> votes = context
                    .Votes
                    .Include(v => v.Choice)
                    .Where(v => v.Choice.PollChoiceNumber == pollOptionNumber)
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

        private void UpdateOptions(ManageChoiceUpdateRequest request, Poll poll, IEnumerable<int> optionsToUpdate)
        {
            foreach (int pollOptionNumber in optionsToUpdate)
            {
                Choice option = poll
                    .Choices
                    .Single(o => o.PollChoiceNumber == pollOptionNumber);

                ChoiceUpdate update = request
                    .Choices
                    .Single(o => o.ChoiceNumber == pollOptionNumber);

                option.Name = update.Name;
                option.Description = update.Description;

                _metricHandler.HandleChoiceUpdatedEvent(option, poll.UUID);
            }
        }

        private void AddNewOptions(IVotingContext context, Poll poll, IEnumerable<ChoiceUpdate> optionUpdatesToAdd)
        {
            foreach (ChoiceUpdate optionRequest in optionUpdatesToAdd)
            {
                var option = new Choice
                {
                    Name = optionRequest.Name,
                    Description = optionRequest.Description
                };

                _metricHandler.HandleChoiceAddedEvent(option, poll.UUID);

                poll.Choices.Add(option);
                context.Choices.Add(option);
            }
        }
    }
}
