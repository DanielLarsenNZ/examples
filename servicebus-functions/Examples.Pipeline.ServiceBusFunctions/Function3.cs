using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Threading.Tasks;

namespace Examples.Pipeline.ServiceBusFunctions
{
    public static class Function3
    {
        [FunctionName("Function3")]
        public static async Task Run(
            [ServiceBusTrigger("queue2", Connection = "ServiceBusConnectionString")]Message message,
            [ServiceBus("queue3", Connection = "ServiceBusConnectionString")]IAsyncCollector<Message> messages,
            ILogger log)
        {
            string messageBody = Encoding.UTF8.GetString(message.Body);

            // Replace these two lines with your processing logic.
            log.LogInformation($"Function3: message = {messageBody}");

            // send processed message to next hub
            var newMessage = new Message(Encoding.UTF8.GetBytes(messageBody));
            await messages.AddAsync(newMessage);
        }
    }
}
