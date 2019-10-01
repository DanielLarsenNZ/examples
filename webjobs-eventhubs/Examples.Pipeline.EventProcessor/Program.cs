using Examples.Pipeline.Insights;
using Microsoft.ApplicationInsights;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Examples.Pipeline.EventProcessor
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Examples.Pipeline.EventProcessor");

            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                //.AddEnvironmentVariables()
                .Build();

            var insights = InsightsHelper.InitializeTelemetryClient(
                config, 
                "Examples-Pipeline-EventProcessor", 
                $"cloudRoleInstance-{Environment.MachineName}");

            var observer = new EventProcessorObserver();

            var host = InitializeEventProcessorHost(config);
            await host.RegisterEventProcessorFactoryAsync(
                new ForwardToQueueEventProcessorFactory(config, insights, observer), 
                new EventProcessorOptions
                {
                    MaxBatchSize = 100,
                    PrefetchCount = 200
                });

            Console.WriteLine($"Ready: {host}");

            while (true)
            {
                await Task.Delay(10000);
                Console.WriteLine("============================");
                Console.WriteLine(observer.GetMetricsString());
            }
        }

        private static EventProcessorHost InitializeEventProcessorHost(IConfiguration config)
        {
            string eventHubConnectionString = config["EventHubConnectionString"];
            string storageConnectionString = config["ServicesStorageConnectionString"];
            string eventHubName = config["EventProcessor.EventHubName"];
            string leaseContainerName = config["EventProcessor.LeaseContainerName"];

            if (string.IsNullOrEmpty(eventHubConnectionString))
                throw new InvalidOperationException("App Setting EventHubConnectionString is missing.");
            if (string.IsNullOrEmpty(storageConnectionString))
                throw new InvalidOperationException("App Setting StorageConnectionString is missing.");
            if (string.IsNullOrEmpty(eventHubName))
                throw new InvalidOperationException("App Setting EventProcessor.EventHubName is missing.");
            if (string.IsNullOrEmpty(leaseContainerName))
                throw new InvalidOperationException("App Setting EventProcessor.LeaseContainerName is missing.");

            var host = new EventProcessorHost(
                eventHubName,
                PartitionReceiver.DefaultConsumerGroupName,
                eventHubConnectionString,
                storageConnectionString,
                leaseContainerName);

            return host;
        }
    }
}
