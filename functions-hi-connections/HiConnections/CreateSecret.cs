using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace HiConnections
{
    public static class CreateSecret
    {
        [FunctionName("CreateSecret")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var crypto = KeyVaultCrypto.NewInstance();
            var cipherText = await crypto.EncryptAsync("Plain Text");

            return new OkObjectResult(cipherText);
        }
    }
}
