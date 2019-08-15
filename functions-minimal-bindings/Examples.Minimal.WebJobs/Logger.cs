using System;
using System.Collections.Generic;
using System.Text;

namespace Examples.Minimal.WebJobs
{
    internal class Logger
    {
        public static void LogInformation(string message)
        {
            //TODO: ILogger
            Console.Out.WriteLine(message);
        }

        public static void LogError(Exception exception, string message)
        {
            //TODO: ILogger
            Console.Error.WriteLine(message);
            if (exception != null) Console.Error.WriteLine(exception.Message);
        }

        public static void LogError(string message) => LogError(null, message);
    }
}
