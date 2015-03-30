using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Models.DBViewModels;

namespace VotingApplication.Web.Api.Controllers.API_Controllers
{
    public class PollOptionController : WebApiController
    {
        public PollOptionController() : base() { }
        public PollOptionController(IContextFactory contextFactory) : base(contextFactory) { }

        private OptionRequestResponseModel OptionToModel(Option option)
        {
            return new OptionRequestResponseModel
            {
                Id = option.Id,
                Name = option.Name,
                Description = option.Description
            };
        }

        private Option ModelToOption(OptionCreationRequestModel model)
        {
            return new Option
            {
                Name = model.Name,
                Description = model.Description
            };
        }

        #region GET


        public List<OptionRequestResponseModel> Get(Guid pollId)
        {
            using (var context = _contextFactory.CreateContext())
            {
                Poll poll = context.Polls.Where(s => s.UUID == pollId).Include(s => s.Options).SingleOrDefault();

                if (poll == null)
                {
                    this.ThrowError(HttpStatusCode.NotFound, string.Format("Poll {0} not found", pollId));
                }

                return poll.Options.Select(OptionToModel).ToList();
            }
        }

        #endregion

        #region POST

        public void Post(Guid pollId, OptionCreationRequestModel optionCreationRequest)
        {
            using (var context = _contextFactory.CreateContext())
            {
                if (optionCreationRequest == null)
                {
                    this.ThrowError(HttpStatusCode.BadRequest);
                }

                Poll poll = context.Polls.Where(p => p.UUID == pollId).FirstOrDefault();
                if (poll == null)
                {
                    this.ThrowError(HttpStatusCode.NotFound, string.Format("Poll {0} does not exist", pollId));
                }

                if (!poll.OptionAdding)
                {
                    this.ThrowError(HttpStatusCode.MethodNotAllowed, string.Format("Option adding not allowed for poll {0}", pollId));
                }

                if (!ModelState.IsValid)
                {
                    this.ThrowError(HttpStatusCode.BadRequest, ModelState);
                }

                Option newOption = ModelToOption(optionCreationRequest);

                if (poll.Options == null)
                {
                    poll.Options = new List<Option>();
                }

                poll.Options.Add(newOption);
                context.SaveChanges();

                return;
            }
        }
        #endregion
    }
}