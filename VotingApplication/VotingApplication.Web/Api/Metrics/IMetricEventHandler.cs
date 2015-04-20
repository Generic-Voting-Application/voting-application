using System;
using System.Net;
using System.Web;
using System.Web.Http;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;

namespace VotingApplication.Web.Api.Metrics
{
    public interface IMetricEventHandler
    {
        void PageChangeEvent(string route, int statusCode, Guid pollId);
        void ErrorEvent(HttpResponseException exception, Guid pollId);

        void UpdateResults(HttpStatusCode status, Guid pollId);
        void SetExpiry(DateTimeOffset? expiry, Guid pollId);

        void LoginEvent();
        void RegisterEvent();
    }
}