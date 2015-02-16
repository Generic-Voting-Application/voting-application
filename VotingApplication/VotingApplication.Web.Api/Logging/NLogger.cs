using System;
using NLog;

namespace VotingApplication.Web.Api.Logging
{
    public class NLogger : ILogger
    {
        private Logger _logger = LogManager.GetLogger("Logger");

        void ILogger.Log(string message)
        {
            _logger.Trace(message);
        }

        void ILogger.Log(Exception exception)
        {
            _logger.Error(exception);
        }

        void ILogger.Log(string message, Exception exception)
        {
            _logger.Error(message, exception);
        }
    }
}