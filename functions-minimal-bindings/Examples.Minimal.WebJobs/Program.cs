using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Logging;
using System;
using System.IO;

namespace Examples.Minimal.WebJobs
{
    class Program
    {
        static IConfiguration Configuration;

        static void Main()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
#if DEBUG
                .AddJsonFile("local.settings.json", optional: false, reloadOnChange: true)
#endif
                .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var builder = new HostBuilder();

            builder.ConfigureWebJobs(b =>
            {    
                b.AddAzureStorageCoreServices();
                b.AddEventHubs(a =>
                {
                    a.BatchCheckpointFrequency = 5;
                    a.EventProcessorOptions.MaxBatchSize = 256;
                    a.EventProcessorOptions.PrefetchCount = 512;
                    a.AddEventProcessorHost(Common.EventHubName, InitializeEventProcessorHost());
                });
            }).ConfigureLogging((context, b) =>
            {
                b.SetMinimumLevel(LogLevel.Trace);
                //b.AddConsole() #BUG
            });

            var host = builder.Build();
            using (host)
            {
                host.Run();
            }
        }

        private static EventProcessorHost InitializeEventProcessorHost()
        {
            string eventHubConnectionString = Configuration["EventHubConnectionString"];
            string storageConnectionString = Configuration["ServicesStorageConnectionString"];
            const string eventHubsStorageContainer = "eventhubs";   //TODO - host/partiion specific?

            if (string.IsNullOrEmpty(eventHubConnectionString))
                throw new InvalidOperationException("App Setting EventHubConnectionString is missing.");

            if (string.IsNullOrEmpty(storageConnectionString))
                throw new InvalidOperationException("App Setting StorageConnectionString is missing.");

            var host = new EventProcessorHost(
                Common.EventHubName,
                PartitionReceiver.DefaultConsumerGroupName,
                eventHubConnectionString,
                storageConnectionString,
                eventHubsStorageContainer);

            // Registers the Event Processor Host and starts receiving messages
            //host.RegisterEventProcessorAsync<SimpleEventProcessor>().GetAwaiter().GetResult();

            return host;
        }
    }
}
