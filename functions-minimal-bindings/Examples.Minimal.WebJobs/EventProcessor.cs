using Examples.Minimal.Commands;
using Examples.Minimal.Helpers;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Text;

namespace Examples.Minimal.WebJobs
{
    public static class EventProcessor
    {
        private static IConfiguration _config = null;

        [FunctionName("EventProcessor")]
        public static void Run(
            [EventHubTrigger(Common.EventHubName)] EventData[] messages,
            ILogger log)
        {
            Console.WriteLine($"C# function triggered to process {messages.Length} messages.");
            log.LogInformation($"C# function triggered to process {messages.Length} messages.");
            //_config = FunctionsHelper.GetConfig(context);
            foreach (EventData message in messages)
            {
                // deserialise
                string data = Encoding.UTF8.GetString(message.Body.Array, message.Body.Offset, message.Body.Count);
                Console.WriteLine(data);

                //TODO: Don't deserialise twice
                dynamic command = JsonConvert.DeserializeObject(data);

                switch (command.CommandType.ToString())
                {
                    case nameof(CreditAccountCommand):
                        command = JsonConvert.DeserializeObject<CreditAccountCommand>(data);
                        break;
                    case nameof(DebitAccountCommand):
                        command = JsonConvert.DeserializeObject<CreditAccountCommand>(data);
                        break;
                    default:
                        throw new NotSupportedException($"\"{command.CommandType}\" is not a supported CommandType.");
                }

                log.LogInformation($"{command}");

                // check for dupes




                // execute the command

            }
        }

        //private static EventProcessorHost InitializeEventProcessorHost()
        //{
        //    string eventHubConnectionString = _config["EventHubConnectionString"];
        //    string storageConnectionString = _config["ServicesStorageConnectionString"];
        //    const string eventHubsStorageContainer = "eventhubs";   //TODO - host/partiion specific?

        //    if (string.IsNullOrEmpty(eventHubConnectionString))
        //        throw new InvalidOperationException("App Setting EventHubConnectionString is missing.");

        //    if (string.IsNullOrEmpty(storageConnectionString))
        //        throw new InvalidOperationException("App Setting StorageConnectionString is missing.");

        //    var host = new EventProcessorHost(
        //        Common.EventHubName,
        //        PartitionReceiver.DefaultConsumerGroupName,
        //        eventHubConnectionString,
        //        storageConnectionString,
        //        eventHubsStorageContainer);

        //    // Registers the Event Processor Host and starts receiving messages
        //    //host.RegisterEventProcessorAsync<SimpleEventProcessor>().GetAwaiter().GetResult();

        //    return host;
        //}
    }
}
