using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector.QuickPulse;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Examples.Pipeline.MessageGenerator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                //.AddEnvironmentVariables()
                .Build();

            var telemetryConfig = TelemetryConfiguration.CreateDefault();
            telemetryConfig.InstrumentationKey = config["APPINSIGHTS_INSTRUMENTATIONKEY"];
            var insights = new TelemetryClient(telemetryConfig);
            insights.TrackTrace("Examples.Pipeline.MessageGenerator.Main");
            var module = new DependencyTrackingTelemetryModule();
            module.IncludeDiagnosticSourceActivities.Add("Microsoft.Azure.ServiceBus");
            module.IncludeDiagnosticSourceActivities.Add("Microsoft.Azure.EventHubs");
            module.Initialize(telemetryConfig);
            telemetryConfig.TelemetryInitializers.Add(new OperationCorrelationTelemetryInitializer());
            QuickPulseTelemetryProcessor processor = null;
            telemetryConfig.TelemetryProcessorChainBuilder
                .Use((next) =>
                {
                    processor = new QuickPulseTelemetryProcessor(next);
                    return processor;
                })
                .Build();

            var QuickPulse = new QuickPulseTelemetryModule();
            QuickPulse.Initialize(telemetryConfig);
            QuickPulse.RegisterTelemetryProcessor(processor);

            int rpm = args.Length > 0 ? int.Parse(args[0]) : 60;


            var client = new QueueClient(config["ServiceBusConnectionString"], config["MessageGenerator.QueueName"]);

            Console.WriteLine($"Sending messages at {rpm} RPM to queue \"{config["MessageGenerator.QueueName"]}\".");
            insights.TrackTrace($"Sending messages at {rpm} RPM to queue \"{config["MessageGenerator.QueueName"]}\".");

            int errors = 0;
            int i = 1;
            while (1 == 1)
            {
                try
                {
                    // Create a new message to send to the queue.
                    string messageBody = $"Message {i}";
                    var message = new Message(Encoding.UTF8.GetBytes(messageBody));

                    // Write the body of the message to the console.
                    Console.WriteLine($"Sending message: {messageBody}");

                    // Send the message to the queue.
                    await client.SendAsync(message);

                    insights.TrackEvent("Examples.Pipeline.MessageGenerator/QueueMessageSent");
                }
                catch (Exception exception)
                {
                    errors++;
                    Console.Error.WriteLine($"Message {i} :: Exception: {exception.Message}");
                    insights.TrackException(exception);
                    if (errors > 10) throw;
                }

                await Task.Delay(60000 / rpm);
                i++;
            }
        }
    }
}
