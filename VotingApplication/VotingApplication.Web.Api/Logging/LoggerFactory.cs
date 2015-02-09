using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VotingApplication.Web.Api.Logging
{
    public class LoggerFactory
    {
        private static ILogger _logger;

        public ILogger GetLogger()
        {
            return _logger ?? new ConsoleLogger();
        }
    }
}