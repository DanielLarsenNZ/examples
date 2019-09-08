using Examples.Pipeline.Commands;
using Examples.Pipeline.Data;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
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
        /// <summary>
        /// Receive batches of Events. Deserialize to Commands and process. 
        /// </summary>
        [FunctionName("EventProcessor")]
        public static async Task Run(
            [EventHubTrigger(Common.EventHubName, Connection = "EventHubConnectionString")] EventData[] events,
            ILogger log,
            PartitionContext partitionContext)
        {
            log.LogInformation($"EventProcessor triggered to process {events.Length} messages.");

            // Can't get DI to work in WebJobs SDK :|
            ITransactionsCommandHandler handler = Program.Services.GetService<ITransactionsCommandHandler>();

            foreach (EventData eventData in events)
            {
                // deserialise
                string data = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);

                //TODO: Don't deserialise twice :(
                dynamic command = JsonConvert.DeserializeObject(data);

                switch (command.CommandType.ToString())
                {
                    case nameof(CreditAccountCommand):
                        command = JsonConvert.DeserializeObject<CreditAccountCommand>(data);
                        break;
                    case nameof(DebitAccountCommand):
                        command = JsonConvert.DeserializeObject<DebitAccountCommand>(data);
                        break;
                    default:
                        throw new NotSupportedException($"\"{command.CommandType}\" is not a supported CommandType.");
                }

                //TODO: check for dupes. Use comand.ComputeHash()


                // execute the command
                await handler.Handle(command);
            }


            log.LogDebug($"PartitionId: {partitionContext.PartitionId}");
            log.LogDebug($"Owner: {partitionContext.Owner}");
            log.LogInformation($"Sum of all Transactions to date Amount: {TransactionsRepository._transactionData.Sum(t => t.Value.Amount)}");
            log.LogInformation($"Sum of all Account Balances: {TransactionsRepository._accountBalanceData.Sum(b => b.Value)}");
            log.LogInformation($"Count of all Transactions: {TransactionsRepository._transactionData.Count}");
        }
    }
}
