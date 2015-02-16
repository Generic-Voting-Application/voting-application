using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VotingApplication.Web.Api.Logging
{
    public static class LoggerFactory
    {
        private static ILogger _logger = new NLogger();

        public static ILogger GetLogger()
        {
            return _logger;
        }
    }
}