using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector.QuickPulse;
using Microsoft.Extensions.Configuration;
using System;

namespace Examples.Pipeline.Insights
{
    public static class InsightsHelper
    {
        public static TelemetryClient InitializeTelemetryClient(IConfiguration config)
        {
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

            return insights;
        }
    }
}
