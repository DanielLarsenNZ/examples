using HealthChecks.AzureKeyVault;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Examples.AppServiceConfig
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddHealthChecks()
                .AddAzureKeyVault(setup => {
                    setup.UseAzureManagedServiceIdentity();
                    setup.UseKeyVaultUrl(Configuration["AzureKeyVaultUrl"]);
                    setup.AddSecret("Rdostr-Mobile--DataKey12");
                }, name: "KeyVault");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHealthChecks("/health", new HealthCheckOptions {
                ResponseWriter = WriteHealthResponse
            });

            app.UseHttpsRedirection();
            app.UseMvc();
        }

        private static Task WriteHealthResponse(HttpContext httpContext, HealthReport result)
        {


            httpContext.Response.ContentType = "application/json";

            //var json = new JObject(
            //    new JProperty("status", result.Status.ToString()),
            //    new JProperty("results", new JObject(result.Entries.Select(pair =>
            //        new JProperty(pair.Key, new JObject(
            //            new JProperty("status", pair.Value.Status.ToString()),
            //            new JProperty("description", pair.Value.Description),
            //            new JProperty("data", new JObject(pair.Value.Data.Select(
            //                p => new JProperty(p.Key, p.Value))))))))));
            return httpContext.Response.WriteAsync(JsonConvert.SerializeObject(MapToHealthReportResult(result)));
        }

        private static HealthReportResult MapToHealthReportResult(HealthReport report)
        {
            var result = new HealthReportResult
            {
                Status = report.Status.ToString(),
                TotalDuration = report.TotalDuration
            };

            foreach(var entryKV in report.Entries)
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
    }

    class HealthReportEntryResult
    {
        public string Description { get; set;  }
        public TimeSpan Duration { get; set; }
        public string ExceptionMessage { get; set; }
        public string Status { get; set;  }
    }
}
