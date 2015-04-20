using System;
using System.Web;
using System.Web.Http;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;

namespace VotingApplication.Web.Api.Metrics
{
    public interface IMetricEventHandler
    {
        Event ErrorEvent(HttpResponseException exception, Guid pollId);
    }
}