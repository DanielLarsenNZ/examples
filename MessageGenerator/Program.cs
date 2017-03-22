using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Threading.Tasks;

namespace MessageGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            Task.Run(async () =>
            {
                await GenerateMessages();
            }).GetAwaiter().GetResult();
        }
        
        /// <summary>
        /// Generate and send a message body to a topic and a queue every 5 seconds.
        /// </summary>
        static async Task GenerateMessages()
        {
            // paste your settings here
            const string connectionString = "";
            const string queueName = "testqueue";
            const string topicName = "orders";

            var factory = MessagingFactory.CreateFromConnectionString(connectionString);
            var manager = NamespaceManager.CreateFromConnectionString(connectionString);

            var queue = await manager.GetQueueAsync(queueName);
            var queueClient = factory.CreateQueueClient(queueName);

            // topics
            var topic = await manager.GetTopicAsync(topicName);
            var topicClient = factory.CreateTopicClient(topic.Path);

            int i = 0;
            while (!Console.KeyAvailable)
            {
                i++;
                var now = DateTime.Now;
                var body = new TestMessageBody { DateTime = now, Text = $"This is message {i}." };

                var queueMessage = new BrokeredMessage(body);
                var topicMessage = new BrokeredMessage(body);

                Console.WriteLine($"{now}: Sending message {i} to Queue: {queueMessage}");
                await queueClient.SendAsync(queueMessage);

                Console.WriteLine($"{now}: Sending message {i} to Topic: {topicMessage}");
                await topicClient.SendAsync(topicMessage);

                await Task.Delay(5000);
            }
        }
    }

    [Serializable]
    public class TestMessageBody
    {
        public string Text { get; set; }
        public DateTime @DateTime { get; set; }
    }
}
