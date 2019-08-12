using Examples.Minimal.Functions.Commands;
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

namespace Examples.Minimal.Functions
{
    public static class NewFile
    {
        private const string EventHubName = "transactions";
        private static IConfiguration _config = null;
        private static ILogger _log = null;

        // lazy pattern for reusing client
        private static readonly Lazy<EventHubClient> _lazyClient = new Lazy<EventHubClient>(InitializeEventHubClient);
        private static EventHubClient EventHubClient => _lazyClient.Value;

        // Don't use AzureWebJobsStorage for data storage in Production
        [FunctionName("NewFile")]
        public static async Task Run(
            [BlobTrigger("data/{name}", Connection = "AzureWebJobsStorage")]Stream blob,
            string name,
            ILogger log,
            ExecutionContext context)
        {
            const int MaxErrorCount = 5;

            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {blob.Length} Bytes");

            _log = log;
            _config = GetConfig(context);

            // Each line in the CSV is a transaction. Create Command as Event Data for each transaction.
            List<EventData> batch = new List<EventData>();

            using (StreamReader reader = new StreamReader(blob))
            {
                int i = 0;
                int errorCount = 0;
                while (reader.Peek() >= 0)
                {
                    i++;

                    // ignore header
                    if (i == 1) continue;

                    // Parse line and create Command
                    TransactionCommand command = null;
                    try
                    {
                        command = ParseLineToCommand(i, reader.ReadLine());
                    }
                    catch (InvalidOperationException ex)
                    {
                        errorCount++;

                        _log.LogError(ex, $"errorCount = {errorCount}. {ex.Message}");

                        if (errorCount > MaxErrorCount) throw;
                    }

                    batch.Add(new EventData(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(command))));
                }
            }

            // Send all transaction events in one batch operation
            // https://docs.microsoft.com/en-us/azure/event-hubs/event-hubs-dotnet-standard-getstarted-send
            await EventHubClient.SendAsync(batch);
        }

        private static TransactionCommand ParseLineToCommand(int lineNumber, string line)
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
                _log.LogError(errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            if (!DateTime.TryParse(fields[DateTimeField], out DateTime dateTime))
            {
                string errorMessage = $"Could not parse date_time to DateTime: line #{lineNumber} field #{DateTimeField} \"{fields[DateTimeField]}\"";
                _log.LogError(errorMessage);
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
                    DebitAmount = amount
                };
            }

            command.Id = Guid.NewGuid();
            command.AccountNumber = fields[AccountNumberField];
            command.AuthorizationCode = fields[AuthorizationField];
            command.MerchantId = fields[MerchantField];
            command.TransactionDateTime = dateTime;
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
                EntityPath = EventHubName
            };

            return EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());
        }

        // helper to load configuration from file or env vars
        private static IConfiguration GetConfig(ExecutionContext context)
        {
            var config = new ConfigurationBuilder()
#if DEBUG
               .SetBasePath(context.FunctionAppDirectory)
               .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
#endif
               .AddEnvironmentVariables()
               .Build();

            return config;
        }
    }
}
