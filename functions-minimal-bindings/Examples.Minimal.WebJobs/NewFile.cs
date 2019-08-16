using Examples.Minimal.Commands;
using Examples.Minimal.Helpers;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Examples.Minimal.WebJobs
{
    public static class NewFile
    {
        private static IConfiguration _config = null;
        private static ILogger _log = null;

        // lazy pattern for reusing client
        private static readonly Lazy<EventHubClient> _lazyClient = new Lazy<EventHubClient>(InitializeEventHubClient);
        private static EventHubClient EventHubClient => _lazyClient.Value;

        [FunctionName("NewFile")]
        public static async Task Run(
            [BlobTrigger("data/{filename}", Connection = "DataStorageConnectionString")]Stream blob,
            string filename,
            ILogger log)
        {
            const int MaxErrorCount = 5;

            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{filename} \n Size: {blob.Length} Bytes");
            Logger.LogInformation($"C# Blob trigger function Processed blob\n Name:{filename} \n Size: {blob.Length} Bytes");

            _log = log;
            //_config = FunctionsHelper.GetConfig(context);
            _config = Program.Configuration; //TODO: Yergh

            // Each line in the CSV is a transaction. Create Command as Event Data for each transaction.
            var batches = new List<EventDataBatch>();
            batches.Add(EventHubClient.CreateBatch());
            int batchNo = 0;
            decimal checksum = 0;
            int i = 0;
            int errorCount = 0;

            using (StreamReader reader = new StreamReader(blob))
            {
                while (reader.Peek() >= 0)
                {
                    i++;

                    if (i == 1)
                    {
                        // ignore header
                        reader.ReadLine();
                        continue;
                    }

                    // Parse line and create Command
                    TransactionCommand command = null;
                    try
                    {
                        command = ParseLineToCommand(filename, i, reader.ReadLine());
                    }
                    catch (InvalidOperationException ex)
                    {
                        errorCount++;

                        Logger.LogError(ex, $"errorCount = {errorCount}. {ex.Message}");

                        if (errorCount > MaxErrorCount) throw;
                    }

                    if (!batches[batchNo].TryAdd(new EventData(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(command)))))
                    {
                        Logger.LogInformation($"Batch {batchNo} is full at line {i}. Adding new batch");
                        // batch is full
                        batches.Add(EventHubClient.CreateBatch());
                        batchNo++;
                        if (!batches[batchNo].TryAdd(new EventData(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(command)))))
                        {
                            throw new InvalidOperationException();
                        }
                    }

                    checksum += command.Amount;
                }
            }

            // Send all transaction events in batched operations
            // https://docs.microsoft.com/en-us/azure/event-hubs/event-hubs-dotnet-standard-getstarted-send
            int eventCount = 0;
            foreach (EventDataBatch batch in batches)
            { 
                Logger.LogInformation($"Sending batch of {batch.Count} events.");
                await EventHubClient.SendAsync(batch);
                eventCount += batch.Count;
            }

            Logger.LogInformation($"Filename: {filename}");
            Logger.LogInformation($"Lines (incl. header): {i}");
            Logger.LogInformation($"Events: {eventCount}");
            Logger.LogInformation($"Batches: {batches.Count}");
            Logger.LogInformation($"Sum of amount: {checksum}");
        }

        private static TransactionCommand ParseLineToCommand(string filename, int lineNumber, string line)
        {
            const int IdField = 0;
            const int AccountNumberField = 1;
            const int DateTimeField = 2;
            const int AmountField = 3;
            const int MerchantField = 4;
            const int AuthorizationField = 5;

            string[] fields = line.Split(',');

            if (!decimal.TryParse(fields[AmountField].Replace("$", ""), out decimal amount))
            {
                string errorMessage = $"Could not parse amount to decimal: line #{lineNumber} field #{AmountField} \"{fields[AmountField]}\"";
                Logger.LogError(errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            if (!DateTime.TryParse(fields[DateTimeField], out DateTime dateTime))
            {
                string errorMessage = $"Could not parse date_time to DateTime: line #{lineNumber} field #{DateTimeField} \"{fields[DateTimeField]}\"";
                Logger.LogError(errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            TransactionCommand command;

            if (amount >= 0)
            {
                // Credit
                command = new CreditAccountCommand
                {
                    CreditAmount = amount
                };
            }
            else
            {
                // Debit
                command = new DebitAccountCommand
                {
                    DebitAmount = Math.Abs(amount)
                };
            }

            command.Id = Guid.NewGuid();
            command.Filename = filename;
            command.AccountNumber = fields[AccountNumberField];
            command.AuthorizationCode = fields[AuthorizationField];
            command.MerchantId = fields[MerchantField];
            command.TransactionDateTime = dateTime;
            command.Amount = amount;
            command.TransactionId = fields[IdField];

            return command;
        }

        private static EventHubClient InitializeEventHubClient()
        {
            string eventHubConnectionString = _config["EventHubConnectionString"];

            if (string.IsNullOrEmpty(eventHubConnectionString))
                throw new InvalidOperationException("App Setting EventHubConnectionString is missing.");

            var connectionStringBuilder = new EventHubsConnectionStringBuilder(eventHubConnectionString)
            {
                EntityPath = Common.EventHubName
            };

            return EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());
        }
    }
}
