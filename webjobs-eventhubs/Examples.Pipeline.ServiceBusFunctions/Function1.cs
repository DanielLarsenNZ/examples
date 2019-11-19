using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Examples.Pipeline.ServiceBusFunctions
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task Run(
            [TimerTrigger("0 */1 * * * *")]TimerInfo myTimer,
            [ServiceBus("queue1", Connection = "ServiceBusConnectionString")]IAsyncCollector<Message> messages,
            ILogger log)
        {
            log.LogInformation($"Function1: executed at: {DateTime.Now}");

            int count = 0;
            for (int i = 1; i < 101; i++)
            {
                var message = new Message(
                        Encoding.UTF8.GetBytes(
                            JsonConvert.SerializeObject(
                                new { Number = i, DateTime = DateTime.UtcNow })));

                // then send the message
                await messages.AddAsync(message);
                count++;
            }

            log.LogInformation($"Function1: Sending batch of {count} messages.");
        }
    }
}
