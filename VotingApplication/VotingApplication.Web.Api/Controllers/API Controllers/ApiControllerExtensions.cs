using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ModelBinding;

namespace VotingApplication.Web.Api.Controllers.API_Controllers
{
    public static class ApiControllerExtensions
    {
        public static void ThrowError(this ApiController controller, HttpStatusCode statusCode, string message = "")
        {
            throw new HttpResponseException(new HttpResponseMessage(statusCode)
            {
                ReasonPhrase = message
            });
        }

        public static void ThrowError(this ApiController controller, HttpStatusCode statusCode, ModelStateDictionary modelState)
        {
            throw new HttpResponseException(controller.Request.CreateErrorResponse(statusCode, modelState));
        }
    }
}