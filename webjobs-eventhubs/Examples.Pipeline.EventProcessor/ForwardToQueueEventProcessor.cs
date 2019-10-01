using Microsoft.ApplicationInsights;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Examples.Pipeline.EventProcessor
{
    class ForwardToQueueEventProcessor : IEventProcessor
    {
        private readonly QueueClient _queueClient;
        private readonly TelemetryClient _insights;
        private readonly IObserver<long> _eventProcessorObserver;

        public ForwardToQueueEventProcessor(IConfiguration config, TelemetryClient insights, IObserver<long> eventProcessorObserver)
        {
            _queueClient = new QueueClient(config["ServiceBusConnectionString"], config["EventProcessor.QueueName"]);
            _insights = insights;
            _eventProcessorObserver = eventProcessorObserver;
        }

        public Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            Console.WriteLine($"Processor Shutting Down. Partition '{context.PartitionId}', Reason: '{reason}'.");
            _insights.TrackTrace($"Processor Shutting Down. Partition '{context.PartitionId}', Reason: '{reason}'.");
            return Task.CompletedTask;
        }

        public Task OpenAsync(PartitionContext context)
        {
            Console.WriteLine($"ForwardToQueueEventProcessor initialized. Partition: '{context.PartitionId}'");
            _insights.TrackTrace($"ForwardToQueueEventProcessor initialized. Partition: '{context.PartitionId}'");
            return Task.CompletedTask;
        }

        public Task ProcessErrorAsync(PartitionContext context, Exception error)
        {
            Console.WriteLine($"Error on Partition: {context.PartitionId}, Error: {error.Message}");
            _insights.TrackException(error);
            return Task.CompletedTask;
        }

        public async Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            Console.WriteLine($"Received {messages.Count()} messages on Partition: '{context.PartitionId}'");

            var batch = new List<Message>();

            foreach (var m in messages)
            {
                Console.WriteLine($"Batching event seq # {m.SystemProperties.SequenceNumber}");
                var props = new Dictionary<string, string>(m.Properties.Select(
                    p => new KeyValuePair<string, string>(p.Key, p.Value.ToString())));
                props.Add(nameof(m.SystemProperties.SequenceNumber), m.SystemProperties.SequenceNumber.ToString());


                _eventProcessorObserver.OnNext(m.SystemProperties.SequenceNumber);
                _insights.TrackEvent("Examples.Pipeline.EventProcessor/EventDataReceived", properties: props);

                var message = new Message(
                    Encoding.UTF8.GetBytes(
                        Encoding.UTF8.GetString(m.Body.Array, m.Body.Offset, m.Body.Count)));

                // record the SequenceNumber
                message.UserProperties.Add("Event.SystemProperties.SequenceNumber", m.SystemProperties.SequenceNumber);

                // copy the user properties
                foreach (var p in m.Properties)
                {
                    message.UserProperties.Add(p.Key, p.Value);
                }

                batch.Add(message);
            }

            Console.WriteLine($"Sending Batch of {batch.Count()} messages");

            await _queueClient.SendAsync(batch);
            await context.CheckpointAsync();
        }
    }
}
