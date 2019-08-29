using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Examples.AppServiceConfig.Helpers
{
    public static class HealthCheckHelper
    {
        public static IApplicationBuilder UseHealthChecks(this IApplicationBuilder app, PathString path, bool jsonResponse)
        {
            return jsonResponse
                ? app.UseHealthChecks(path, new HealthCheckOptions
                {
                    ResponseWriter = WriteHealthResponse
                })
                : app.UseHealthChecks(path);
        }

        public static Task WriteHealthResponse(HttpContext httpContext, HealthReport result)
        {
            httpContext.Response.ContentType = "application/json";
            return httpContext.Response.WriteAsync(JsonConvert.SerializeObject(MapToHealthReportResult(result)));
        }

        private static HealthReportResult MapToHealthReportResult(HealthReport report)
        {
            var result = new HealthReportResult
            {
                Status = report.Status.ToString(),
                TotalDuration = report.TotalDuration,
                DateTimeUtc = DateTime.UtcNow
            };

            foreach (var entryKV in report.Entries)
            {
                var entry = entryKV.Value;

                result.Entries.Add(entryKV.Key, new HealthReportEntryResult
                {
                    Description = entry.Description,
                    Duration = entry.Duration,
                    ExceptionMessage = entry.Exception?.Message,
                    Status = entry.Status.ToString()
                });
            }

            return result;
        }
    }

    class HealthReportResult
    {
        public HealthReportResult()
        {
            Entries = new Dictionary<string, HealthReportEntryResult>();
        }

        public string Status { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public Dictionary<string, HealthReportEntryResult> Entries { get; set; }
        public DateTime DateTimeUtc { get; set; }
    }

    class HealthReportEntryResult
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }
        public TimeSpan Duration { get; set; }
        public string ExceptionMessage { get; set; }
        public string Status { get; set; }
    }
}
