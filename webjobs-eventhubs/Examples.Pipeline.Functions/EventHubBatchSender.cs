using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Examples.Pipeline.Functions
{
    public class EventHubBatchSender
    {
        private static IConfiguration _config = null;

        // lazy pattern for reusing client
        private static readonly Lazy<EventHubClient> _lazyClient = new Lazy<EventHubClient>(InitializeEventHubClient);
        private static EventHubClient EventHubClient => _lazyClient.Value;

        [FunctionName("EventHubBatchSender")]
        public async Task Run(
            [TimerTrigger("0 */1 * * * *")]TimerInfo timer,
            ILogger log,
            ExecutionContext context)
        {
            log.LogInformation($"EventHubBatchSender function executed at: {DateTime.Now}");

            _config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var batch = EventHubClient.CreateBatch();

            for (int i = 1; i < 101; i++)
            {
                var eventData = new EventData(
                        Encoding.UTF8.GetBytes(
                            JsonConvert.SerializeObject(
                                new { Number = i, DateTime = DateTime.UtcNow })));

                if (!batch.TryAdd(eventData))
                {
                    break;
                }
            }

            log.LogInformation($"Sending batch of {batch.Count} events.");
            await EventHubClient.SendAsync(batch);
        }

        private static EventHubClient InitializeEventHubClient()
        {
            string eventHubConnectionString = _config["EventHubConnectionString"];

            if (string.IsNullOrEmpty(eventHubConnectionString))
                throw new InvalidOperationException("App Setting EventHubConnectionString is missing.");

            var connectionStringBuilder = new EventHubsConnectionStringBuilder(eventHubConnectionString)
            {
                EntityPath = "numbers-batched"
            };

            return EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());
        }
    }
}
