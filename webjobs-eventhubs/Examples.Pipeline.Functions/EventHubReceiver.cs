using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Examples.Pipeline.Functions
{
    public class EventHubReceiver
    {
        readonly TelemetryClient _telemetry;

        public EventHubReceiver(TelemetryConfiguration telemetryConfiguration)
        {
            _telemetry = new TelemetryClient(telemetryConfiguration);
        }

        [FunctionName("EventHubReceiver")]
        public void Run(
            [EventHubTrigger("numbers", Connection = "EventHubConnectionString")] EventData eventData,
            ILogger log,
            PartitionContext partitionContext)
        {
            try
            {
                string messageBody = Encoding.UTF8.GetString(
                    eventData.Body.Array,
                    eventData.Body.Offset,
                    eventData.Body.Count);

                // Replace these two lines with your processing logic.
                log.LogInformation($"EventHubReceiver: Partition = {partitionContext.PartitionId}, Owner = {partitionContext.Owner}, message = {messageBody}");

                _telemetry.TrackEvent(
                    "EventHubReceiver/EventProcessed", 
                    properties: new Dictionary<string, string>
                    {
                        { "partitionId", partitionContext.PartitionId },
                        { "owner", partitionContext.Owner },
                        { "sequenceNumber", eventData.SystemProperties.SequenceNumber.ToString() }
                    });
            }
            catch (Exception e)
            {
                // We need to keep processing the rest of the batch - capture this exception and continue.
                // Also, consider capturing details of the message that failed processing so it can be processed again later.
                _telemetry.TrackException(e);
                throw;
            }
        }
    }
}
