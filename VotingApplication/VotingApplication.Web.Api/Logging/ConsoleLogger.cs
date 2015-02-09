using System;

namespace VotingApplication.Web.Api.Logging
{
    public class ConsoleLogger : ILogger
    {
        void ILogger.Log(string message)
        {
            Console.Out.WriteLine(message);
        }

        void ILogger.Log(Exception exception)
        {
            Console.Error.WriteLine(exception.Message);
        }

        void ILogger.Log(string message, Exception exception)
        {
            Console.Error.WriteLine(message + " \n " + exception.Message);
        }
    }
}