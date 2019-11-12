using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights;

namespace Examples.Pipeline.Functions
{
    public static class MessageGenerator
    {
        private static readonly Lazy<QueueClient> _lazyQueueClient = new Lazy<QueueClient>(InitializeQueueClient);
        private static QueueClient QueueClient => _lazyQueueClient.Value;
        private static IConfiguration _config;


        //[FunctionName("MessageGenerator")]
        public static async Task Run([TimerTrigger("0 */1 * * * *")]TimerInfo timer, 
            ILogger log,
            ExecutionContext context)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            const int rpm = 60;

            _config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            try
            {
                for (var i = 0; i < rpm; i++)
                {
                    // Create a new message to send to the queue.
                    string messageBody = $"Message {i}";
                    var message = new Message(Encoding.UTF8.GetBytes(messageBody));

                    // Write the body of the message to the console.
                    Console.WriteLine($"Sending message: {messageBody}");

                    // Send the message to the queue.
                    await QueueClient.SendAsync(message);

                    await Task.Delay(60000 / rpm);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"{DateTime.Now} :: Exception: {exception.Message}");
            }
        }

        private static QueueClient InitializeQueueClient()
        {
            return new QueueClient(_config["ServiceBusConnectionString"], _config["MessageGenerator.QueueName"]);
        }
    }
}
