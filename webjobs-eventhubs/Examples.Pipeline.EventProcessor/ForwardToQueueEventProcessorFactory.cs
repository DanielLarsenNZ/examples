using Microsoft.ApplicationInsights;
using Microsoft.Azure.EventHubs.Processor;
using Microsoft.Extensions.Configuration;
using System;

namespace Examples.Pipeline.EventProcessor
{
    class ForwardToQueueEventProcessorFactory : IEventProcessorFactory
    {
        private readonly IConfiguration _config;
        private readonly TelemetryClient _insights;
        private readonly IObserver<long> _observer;

        public ForwardToQueueEventProcessorFactory(IConfiguration config, TelemetryClient insights, IObserver<long> eventProcessorObserver)
        {
            _config = config;
            _insights = insights;
            _observer = eventProcessorObserver;
        }

        public IEventProcessor CreateEventProcessor(PartitionContext context)
        {
            Console.WriteLine($"Creating ForwardToQueueEventProcessor for {context} PartitionId = {context.PartitionId}");
            return new ForwardToQueueEventProcessor(_config, _insights, _observer);
        }
    }
}
