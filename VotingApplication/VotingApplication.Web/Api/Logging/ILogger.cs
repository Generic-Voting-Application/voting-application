using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VotingApplication.Web.Api.Logging
{
    public interface ILogger
    {
        void Log(String message);
        void Log(Exception exception);
        void Log(String message, Exception exception);
    }
}
