using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;

namespace VotingApplication.Web.Api.Metrics
{
    public class MetricEventHandler : IMetricEventHandler
    {
        private readonly IContextFactory _contextFactory;
        
        public MetricEventHandler (IContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        #region Error Events

        public void ErrorEvent(HttpResponseException exception, Guid pollId)
        {
            Event errorEvent = new Event("ERROR", pollId);

            errorEvent.Value = exception.Response.StatusCode.ToString();

            if (exception.Response.RequestMessage != null)
            {
                errorEvent.Detail = ErrorDetailFromRequestMessage(exception);
            }
            else
            {
                errorEvent.Detail = exception.Response.ReasonPhrase;
            }

            StoreEvent(errorEvent);
        }

        private string ErrorDetailFromRequestMessage(HttpResponseException exception)
        {
            string apiRoute = exception.Response.RequestMessage.Method + " " + exception.Response.RequestMessage.RequestUri;
            string requestPayload = GetPayload(exception);
            return apiRoute + " " + requestPayload;
        }

        private string GetPayload(HttpResponseException exception)
        {
            var requestContext = exception.Response.RequestMessage.Properties["MS_RequestContext"];
            HttpRequestWrapper webRequest = requestContext.GetType().GetProperty("WebRequest").GetValue(requestContext) as HttpRequestWrapper;
            
            if (webRequest == null)
            {
                return "";
            }

            byte[] buffer = new byte[webRequest.ContentLength];
            webRequest.InputStream.Read(buffer, 0, webRequest.ContentLength);
            return System.Text.Encoding.Default.GetString(buffer);
        }

        #endregion

        public void PageChangeEvent(string route, int statusCode, Guid pollId)
        {
            Event pageChangeEvent = new Event("GoTo" + route, pollId);
            pageChangeEvent.Value = ((HttpStatusCode)statusCode).ToString();
            StoreEvent(pageChangeEvent);
        }

        private void StoreEvent(Event eventToStore)
        {
            using (var context = _contextFactory.CreateContext())
            {
                context.Events.Add(eventToStore);
                context.SaveChanges();
            }
        }
    }
}