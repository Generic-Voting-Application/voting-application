using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Web.Http;

namespace VotingApplication.Web.Api.Tests.Controllers
{
    public class ExpectedHttpResponseExceptionAttribute : ExpectedExceptionBaseAttribute
    {
        private HttpStatusCode _statusCode;

        public ExpectedHttpResponseExceptionAttribute(HttpStatusCode statusCode)
        {
            _statusCode = statusCode;
        }

        protected override void Verify(Exception exception)
        {
            HttpResponseException e = exception as HttpResponseException;

            if(e == null)
            {
                throw new Exception("Expected HttpResponseException not thrown");
            }

            if(e.Response == null || e.Response.StatusCode != _statusCode)
            {
                throw new Exception(String.Format("Expected Status code to equal {0}, Actual: {1}", 
                                                  _statusCode.ToString(), 
                                                  e.Response.StatusCode.ToString()));
            }
        } 
    }
}
