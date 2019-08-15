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
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
#if DEBUG
                .AddJsonFile($"appsettings.json", optional: false, reloadOnChange: true)
#endif
                .AddEnvironmentVariables()
                .Build();

            var builder = new HostBuilder();

            Console.WriteLine($"Configuration.GetConnectionString(\"Storage\") = {Configuration.GetConnectionString("Storage")}");
            Console.WriteLine($"Configuration.GetConnectionString(\"DataStorageConnectionString\") = {Configuration.GetConnectionString("DataStorageConnectionString")}");
            Console.WriteLine($"Configuration.GetConnectionString(\"AzureWebJobsDataStorageConnectionString\") = {Configuration.GetConnectionString("AzureWebJobsDataStorageConnectionString")}");

            builder.ConfigureWebJobs(b =>
            {    
                b.AddAzureStorageCoreServices();
                b.AddAzureStorage();
                b.AddEventHubs(h =>
                {
                    h.BatchCheckpointFrequency = 5;
                    h.EventProcessorOptions.MaxBatchSize = 256;
                    h.EventProcessorOptions.PrefetchCount = 512;
                    h.AddEventProcessorHost(Common.EventHubName, InitializeEventProcessorHost());
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
