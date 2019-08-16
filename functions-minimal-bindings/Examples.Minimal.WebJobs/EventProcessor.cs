using Examples.Minimal.Commands;
using Examples.Minimal.Data;
using Examples.Minimal.Helpers;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Examples.Minimal.WebJobs
{
    public static class EventProcessor
    {
        //private static IConfiguration _config = null;
        private static ITransactionsCommandHandler _handler = new TransactionsCommandHandler();

        [FunctionName("EventProcessor")]
        public static async Task Run(
            [EventHubTrigger(Common.EventHubName, Connection = "EventHubConnectionString")] EventData[] messages,
            ILogger log)
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

                //log.LogInformation($"{command}");

                // check for dupes




                // execute the command
                await _handler.Handle(command);
            }

            Logger.LogInformation($"Events: {messages.Length}");
            Logger.LogInformation($"Sum of all Transactions to date Amount: {TransactionsRepository._transactionData.Sum(t=>t.Value.Amount)}");
            Logger.LogInformation($"Sum of all Account Balances: {TransactionsRepository._accountBalanceData.Sum(b=>b.Value)}");
        }
    }
}
