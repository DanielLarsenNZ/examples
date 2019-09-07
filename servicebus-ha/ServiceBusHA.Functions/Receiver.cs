using System;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ServiceBusHA.Functions
{
    public static class Receiver
    {
        const string ServiceBusConnectionString = "ServiceBusConnectionString";
        const string DeDupeTableName = "MessagesReceived";

        private static readonly Lazy<CloudTableClient> _lazyClient = new Lazy<CloudTableClient>(InitializeCloudTableClient);
        private static CloudTableClient CloudTableClient => _lazyClient.Value;

        [FunctionName("Receiver")]
        public static void Run(
            [ServiceBusTrigger("%ServiceBusQueueName%", Connection = "ServiceBusConnectionString")]string message, 
            ILogger log)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {message}");

            //var messageInfo = new MessageInfo(message.)
            
            var table = CloudTableClient.GetTableReference(DeDupeTableName);

        }

        private static CloudTableClient InitializeCloudTableClient()
        {
            var config = new ConfigurationBuilder().Build();

            return CloudStorageAccount
                .Parse(config["ServiceBusConnectionString"])
                .CreateCloudTableClient(new TableClientConfiguration());
        }


    }

    public class MessageInfo : TableEntity
    {
        public MessageInfo()
        {
        }

        public MessageInfo(string messageId)
        {
            PartitionKey = messageId;
            RowKey = messageId;
        }
    }
}
