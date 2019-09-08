using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace Examples.Pipeline.Functions
{
    public static class Function1
    {
        [FunctionName("Function1")]
        [return: EventHub("outputEventHubMessage", Connection = "EventHubConnectionAppSetting")]
        public static void Run([ServiceBusTrigger("myqueue", Connection = "")]string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");

            //TODO: Test dependency tracking
        }
    }
}
