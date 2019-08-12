using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Examples.Minimal.Functions.Helpers;
using Examples.Minimal.Functions.EventHubs;

namespace Examples.Minimal.Functions
{
    public static class EventProcessor
    {
        private static IConfiguration _config = null;

        // lazy pattern for reusing client
        private static readonly Lazy<EventProcessorHost> _lazyClient = new Lazy<EventProcessorHost>(InitializeEventProcessorHost);
        private static EventProcessorHost EventProcessorHost => _lazyClient.Value;

        [FunctionName("EventProcessor")]
        public static async Task Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, ILogger log, ExecutionContext context)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            _config = FunctionsHelper.GetConfig(context);

            await EventProcessorHost.RegisterEventProcessorAsync<SimpleEventProcessor>();
            await Task.Delay(60000);    //TODO: Nope
            await EventProcessorHost.UnregisterEventProcessorAsync();
        }

        private static EventProcessorHost InitializeEventProcessorHost()
        {
            string eventHubConnectionString = _config["EventHubConnectionString"];
            string storageConnectionString = _config["ServicesStorageConnectionString"];
            const string eventHubsStorageContainer = "eventhubs";   //TODO - host/partiion specific?

            if (string.IsNullOrEmpty(eventHubConnectionString))
                throw new InvalidOperationException("App Setting EventHubConnectionString is missing.");

            if (string.IsNullOrEmpty(storageConnectionString))
                throw new InvalidOperationException("App Setting StorageConnectionString is missing.");

            var host = new EventProcessorHost(
                Common.EventHubName,
                PartitionReceiver.DefaultConsumerGroupName,
                eventHubConnectionString,
                storageConnectionString,
                eventHubsStorageContainer);

            // Registers the Event Processor Host and starts receiving messages
            //host.RegisterEventProcessorAsync<SimpleEventProcessor>().GetAwaiter().GetResult();

            return host;
        }
    }
}
