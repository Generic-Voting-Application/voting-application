using System;

namespace VotingApplication.Web.Api.Logging
{
    public class ConsoleLogger : ILogger
    {
        public void Log(string message)
        {
            Console.Out.WriteLine(message);
        }

        public void Log(Exception exception)
        {
            Console.Error.WriteLine(exception.Message);
        }

        public void Log(string message, Exception exception)
        {
            Console.Error.WriteLine(message + " \n " + exception.Message);
        }
    }
}