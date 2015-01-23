using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
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
                Info = option.Info,
                Description = option.Description
            };
        }

        private Option ModelToOption(OptionCreationRequestModel model)
        {
            return new Option
            {
                Name = model.Name,
                Info = model.Info,
                Description = model.Description
            };
        }

        #region GET

        public virtual HttpResponseMessage Get(Guid pollId)
        {
            #region DB Get / Validation

            Poll poll;
            using (var context = _contextFactory.CreateContext())
            {
                poll = context.Polls.Where(s => s.UUID == pollId).Include(s => s.Options).SingleOrDefault();
            }

            if (poll == null)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("Poll {0} not found", pollId));
            }

            #endregion

            #region Response

            return this.Request.CreateResponse(HttpStatusCode.OK, poll.Options.Select(OptionToModel).ToList());

            #endregion
        }

        public virtual HttpResponseMessage Get(Guid pollId, long optionId)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use GET by id on this controller");
        }

        #endregion

        #region POST

        public virtual HttpResponseMessage Post(Guid pollId, OptionCreationRequestModel optionCreationRequest)
        {
            #region Input Validation

            if (optionCreationRequest == null)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            using (var context = _contextFactory.CreateContext())
            {
                Poll poll = context.Polls.Where(p => p.UUID == pollId).FirstOrDefault();
                if (poll == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("Poll {0} does not exist", pollId));
                }

                if (!poll.OptionAdding)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, string.Format("Option adding not allowed for poll {0}", pollId));
                }
            }

            if (!ModelState.IsValid)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, ModelState);
            }

            #endregion

            #region DB Object Creation

            Option newOption = ModelToOption(optionCreationRequest);

            using (var context = _contextFactory.CreateContext())
            {
                Poll poll = context.Polls.Where(p => p.UUID == pollId).Single();
                if (poll.Options == null)
                {
                    poll.Options = new List<Option>();
                }

                poll.Options.Add(newOption);
                context.SaveChanges();
            }

            #endregion

            #region Response

            return this.Request.CreateResponse(HttpStatusCode.OK);

            #endregion
        }

        public virtual HttpResponseMessage Post(Guid pollId, long optionId, Option option)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use POST by id on this controller");
        }

        #endregion

        #region PUT

        public virtual HttpResponseMessage Put(Guid pollId, Option option)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use PUT on this controller");
        }

        public virtual HttpResponseMessage Put(Guid pollId, long optionId, Option option)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use PUT by id on this controller");
        }

        #endregion

        #region DELETE

        public virtual HttpResponseMessage Delete(Guid pollId)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use DELETE on this controller");
        }

        public virtual HttpResponseMessage Delete(Guid pollId, long optionId)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use DELETE by id on this controller");
        }

        #endregion

    }
}