using Microsoft.Extensions.Hosting;

namespace Examples.Minimal.Functions
{
    class Program
    {
        static void Main()
        {
            var builder = new HostBuilder();
            builder.ConfigureWebJobs(b =>
            {
                b.AddAzureStorageCoreServices();
                b.AddEventHubs(a =>
                {
                    a.BatchCheckpointFrequency = 5;
                    a.EventProcessorOptions.MaxBatchSize = 256;
                    a.EventProcessorOptions.PrefetchCount = 512;
                });
            });

            var host = builder.Build();
            using (host)
            {
                host.Run();
            }
        }
    }
}
