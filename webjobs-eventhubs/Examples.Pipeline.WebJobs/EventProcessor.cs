using Examples.Pipeline.Commands;
using Examples.Pipeline.Data;
using Examples.Pipeline.Helpers;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Examples.Pipeline.WebJobs
{
    public static class EventProcessor
    {
        //private static IConfiguration _config = null;
        private static ITransactionsCommandHandler _handler = new TransactionsCommandHandler();

        [FunctionName("EventProcessor")]
        public static async Task Run(
            [EventHubTrigger(Common.EventHubName, Connection = "EventHubConnectionString")] EventData[] messages,
            ILogger log,
            PartitionContext partitionContext)
        {
            log.LogInformation($"C# function triggered to process {messages.Length} messages.");
            Logger.LogInformation($"C# function triggered to process {messages.Length} messages.");
            //_config = FunctionsHelper.GetConfig(context);
            foreach (EventData message in messages)
            {
                // deserialise
                string data = Encoding.UTF8.GetString(message.Body.Array, message.Body.Offset, message.Body.Count);

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

                //TODO: check for dupes


                // execute the command
                await _handler.Handle(command);
            }


            Logger.LogInformation($"PartitionId: {partitionContext.PartitionId}");
            //Logger.LogInformation($"offset: {offset}");
            //Logger.LogInformation($"sequenceNumber: {sequenceNumber}");
            Logger.LogInformation($"Owner: {partitionContext.Owner}");
            Logger.LogInformation($"Sum of all Transactions to date Amount: {TransactionsRepository._transactionData.Sum(t=>t.Value.Amount)}");
            Logger.LogInformation($"Sum of all Account Balances: {TransactionsRepository._accountBalanceData.Sum(b=>b.Value)}");
            Logger.LogInformation($"Count of all Transactions: {TransactionsRepository._transactionData.Count}");
        }
    }
}
