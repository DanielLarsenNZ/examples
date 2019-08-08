using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Examples.Minimal.Functions
{
    public static class NewFile
    {
        private const string EventHubName = "transactions";
        private static IConfiguration _config = null;

        // lazy pattern for reusing client
        private static readonly Lazy<EventHubClient> _lazyClient = new Lazy<EventHubClient>(InitializeEventHubClient);
        private static EventHubClient EventHubClient => _lazyClient.Value;

        // Don't use AzureWebJobsStorage for data storage in Production
        [FunctionName("NewFile")]
        public static async Task Run(
            [BlobTrigger("data/{name}", Connection = "AzureWebJobsStorage")]Stream blob,
            string name,
            ILogger log,
            ExecutionContext context)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {blob.Length} Bytes");

            _config = GetConfig(context);

            // Each line in the CSV is a transaction. Create Event Data for each transaction.
            List<EventData> batch = new List<EventData>();

            using (StreamReader reader = new StreamReader(blob))
            {
                int i = 0;
                while (reader.Peek() >= 0)
                {
                    i++;

                    // ignore header
                    if (i == 1) continue;

                    batch.Add(new EventData(Encoding.UTF8.GetBytes(reader.ReadLine())));
                }
            }

            // Send all transaction events in one batch operation
            await EventHubClient.SendAsync(batch);
        }

        private static EventHubClient InitializeEventHubClient()
        {
            string eventHubConnectionString = _config["EventHubConnectionString"];

            if (string.IsNullOrEmpty(eventHubConnectionString))
                throw new InvalidOperationException("App Setting EventHubConnectionString is missing.");

            var connectionStringBuilder = new EventHubsConnectionStringBuilder(eventHubConnectionString)
            {
                EntityPath = EventHubName
            };

            return EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());
        }

        // helper to load configuration from file or env vars
        private static IConfiguration GetConfig(ExecutionContext context)
        {
            var config = new ConfigurationBuilder()
#if DEBUG
               .SetBasePath(context.FunctionAppDirectory)
               .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
#endif
               .AddEnvironmentVariables()
               .Build();

            return config;
        }
    }
}
