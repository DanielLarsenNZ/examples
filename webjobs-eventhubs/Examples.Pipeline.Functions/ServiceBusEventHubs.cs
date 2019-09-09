using System;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace Examples.Pipeline.Functions
{
    public static class ServiceBusEventHubs
    {
        [FunctionName("ServiceBusEventHubs")]
        [return: EventHub("transactions", Connection = "EventHubConnectionString")]
        public static EventData Run(
            [ServiceBusTrigger("test", Connection = "ServiceBusConnectionString")]Message message,
            ILogger log,
            string messageId)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {messageId}");

            return new EventData(message.Body);
        }
    }
}
