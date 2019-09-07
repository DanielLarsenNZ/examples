using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;

namespace ServiceBusHA.Functions.Helpers
{
    internal class FunctionsHelper
    {
        public static IConfiguration GetConfig()
        {
            var config = new ConfigurationBuilder()
               .Build();

            return config;
        }
    }
}
