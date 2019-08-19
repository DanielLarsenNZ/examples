using Examples.Pipeline.Commands;
using Examples.Pipeline.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging.Debug;
using System;
using System.Threading.Tasks;

namespace Examples.Pipeline.WebJobs
{
    class Program
    {
        public static IServiceProvider Services;

        static async Task Main()
        {
            var builder = new HostBuilder();

            builder.ConfigureWebJobs(b =>
            {
                b.AddAzureStorageCoreServices();
                b.AddAzureStorage();
                b.AddEventHubs(h =>
                {
                    h.BatchCheckpointFrequency = 1; // Checkpoint after every batch processed
                    h.EventProcessorOptions.MaxBatchSize = 100;
                    h.EventProcessorOptions.PrefetchCount = (h.EventProcessorOptions.MaxBatchSize * 2);
                });
            });

            builder.ConfigureServices((context, s) =>
            {
                // https://xfischer.github.io/logging-dotnet-core/
                s.AddLogging(config =>
                {
                    config.AddDebug(); // Log to debug (debug window in Visual Studio or any debugger attached)
                    config.AddConsole(); // Log to console (colored !)
                    // https://docs.microsoft.com/en-us/azure/app-service/webjobs-sdk-get-started#add-application-insights-logging
                    config.AddApplicationInsights(i=>i.InstrumentationKey = context.Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"]);
                })
                .Configure<LoggerFilterOptions>(options =>
                {
                    options.AddFilter<DebugLoggerProvider>(null /* category*/ , LogLevel.Debug /* min level */);
                    options.AddFilter<ConsoleLoggerProvider>(null  /* category*/ , LogLevel.Information /* min level */);
                });

                // Register services
                s.AddSingleton<ITransactionsCommandHandler, TransactionsCommandHandler>();
                s.AddSingleton<ITransactionsRepository, TransactionsRepository>();
            });

            var host = builder.Build();
            Services = host.Services;

            using (host)
            {
                // WebJobs Host will run and block here
                await host.RunAsync();
            }
        }
    }
}
