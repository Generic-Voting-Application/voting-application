using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;

namespace VotingApplication.Web.Api.Metrics
{
    public static class MetricEventHandler
    {
        private static IContextFactory _contextFactory = new ContextFactory();

        public static Event ErrorEvent(HttpResponseException exception, Guid pollId)
        {
            Event errorEvent = new Event("ERROR", pollId);

            using (var context = _contextFactory.CreateContext())
            {
                context.Events.Add(errorEvent);
                context.SaveChanges();
            }

            return errorEvent;
        }
    }
}