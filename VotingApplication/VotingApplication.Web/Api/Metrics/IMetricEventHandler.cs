using System;
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
    }
}