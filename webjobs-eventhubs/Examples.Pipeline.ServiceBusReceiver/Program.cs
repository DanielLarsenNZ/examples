using Examples.Pipeline.Insights;
using Microsoft.ApplicationInsights;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Examples.Pipeline.ServiceBusReceiver
{
    class Program
    {
        // Connection String for the namespace can be obtained from the Azure portal under the 
        // 'Shared Access policies' section.
        static IQueueClient _queueClient;
        static TelemetryClient _insights;

        static async Task Main()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                //.AddEnvironmentVariables()
                .Build();

            _insights = InsightsHelper.InitializeTelemetryClient(
                config,
                "Examples.Pipeline.ServiceBusReceiver",
                $"cloudRoleInstance-{Environment.MachineName}"
                );

            _queueClient = new QueueClient(config["ServiceBusConnectionString"], config["ServiceBusReceiver.QueueName"]);


            // Register QueueClient's MessageHandler and receive messages in a loop
            RegisterOnMessageHandlerAndReceiveMessages(_queueClient);

            Console.WriteLine($"Ready: {_queueClient}");

            while (true)
            {
                await Task.Delay(200);
            }

            //await _queueClient.CloseAsync();
        }

        static void RegisterOnMessageHandlerAndReceiveMessages(IQueueClient queueClient)
        {
            // Configure the MessageHandler Options in terms of exception handling, number of concurrent messages to deliver etc.
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                // Maximum number of Concurrent calls to the callback `ProcessMessagesAsync`, set to 1 for simplicity.
                // Set it according to how many messages the application wants to process in parallel.
                MaxConcurrentCalls = 10,

                // Indicates whether MessagePump should automatically complete the messages after returning from User Callback.
                // False below indicates the Complete will be handled by the User Callback as in `ProcessMessagesAsync` below.
                AutoComplete = false
            };

            // Register the function that will process messages
            //queue1Client.RegisterMessageHandler(ProcessMessages1Async, messageHandlerOptions);
            queueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
        }

        static async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            // Process the message
            Console.WriteLine($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");
            var props = new Dictionary<string, string>(message.UserProperties.Select(p => new KeyValuePair<string, string>(p.Key, p.Value.ToString())));
            props.Add(nameof(message.SystemProperties.SequenceNumber), message.SystemProperties.SequenceNumber.ToString());
            _insights.TrackEvent("Examples.Pipeline.MessageGenerator/QueueMessageReceived", properties: props);

            // Complete the message so that it is not received again.
            // This can be done only if the queueClient is created in ReceiveMode.PeekLock mode (which is default).
            await _queueClient.CompleteAsync(message.SystemProperties.LockToken);

            // Note: Use the cancellationToken passed as necessary to determine if the queueClient has already been closed.
            // If queueClient has already been Closed, you may chose to not call CompleteAsync() or AbandonAsync() etc. calls 
            // to avoid unnecessary exceptions.
        }

        static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            _insights.TrackException(exceptionReceivedEventArgs.Exception);
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            Console.WriteLine("Exception context for troubleshooting:");
            Console.WriteLine($"- Endpoint: {context.Endpoint}");
            Console.WriteLine($"- Entity Path: {context.EntityPath}");
            Console.WriteLine($"- Executing Action: {context.Action}");
            return Task.CompletedTask;
        }
    }
}
