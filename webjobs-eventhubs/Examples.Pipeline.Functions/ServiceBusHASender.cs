using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Examples.Pipeline.Functions
{
    public static class ServiceBusHASender
    {
        //[FunctionName("ServiceBusHASender")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");


            /*
             Set MessageId
             Set UserProperties:
                OriginalMessageId
                OriginalNamespace
                OriginalQueue | OriginalSubscription
             Send to Primary namespace
             If fails after retries, resend to Secondary
             
            The problem is still how do we detect duplicates across regions (if this is required)
            I think the client has to hold the cursor and manage the resending????? :/

             */














            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            return name != null
                ? (ActionResult)new OkObjectResult($"Hello, {name}")
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }
    }
}
