using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Examples.Pipeline.Helpers
{
    public static class WebJobsHelper
    {
        // helper to load configuration from file or env vars
        public static IConfiguration GetConfig(ExecutionContext context)
        {
            var config = new ConfigurationBuilder()
#if DEBUG
               .SetBasePath(context.FunctionAppDirectory)
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
#endif
               .AddEnvironmentVariables()
               .Build();

            return config;
        }
    }
}
