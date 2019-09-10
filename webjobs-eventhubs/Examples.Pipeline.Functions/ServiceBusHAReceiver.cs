using System;
using System.Threading.Tasks;
using Examples.Pipeline.Commands;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Build.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ServiceBusHA.Functions
{
    public static class ServiceBusHAReceiver
    {
        const string ServiceBusConnectionString = "ServiceBusConnectionString";
        const string DeDupeTableName = "MessagesReceived";

        private static readonly Lazy<CloudTableClient> _lazyClient = new Lazy<CloudTableClient>(InitializeCloudTableClient);
        private static CloudTableClient CloudTableClient => _lazyClient.Value;

        //[FunctionName("ServiceBusHAReceiver")]
        public static void Run(
            [ServiceBusTrigger("test1", Connection = "ServiceBusConnectionString")]Message message,
            ILogger log
            )
        {
            log.LogInformation($"C# ServiceBus queue trigger function processing message: id = {message.MessageId}");

            //var command = JsonConvert.DeserializeObject<CreditAccountCommand>(message);

            //var messageInfo = new MessageInfo(message.)

            //var table = CloudTableClient.GetTableReference(DeDupeTableName);

            

        }

        private static async Task<bool> DetectDuplicate(Message message)
        {
            throw new NotImplementedException();
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
