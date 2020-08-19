using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using static Microsoft.Azure.KeyVault.KeyVaultClient;

namespace HiConnections
{
    public static class CreateSecret
    {
        private static readonly KeyVaultClient _keyVaultClient =
            new KeyVaultClient(
                new AuthenticationCallback(new AzureServiceTokenProvider().KeyVaultTokenCallback));

        [FunctionName("CreateSecret")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var crypto = new KeyVaultCrypto(_keyVaultClient, "https://helloase-aue-kv.vault.azure.net/keys/key1");
            var cipherText = await crypto.EncryptAsync("Plain Text");

            return new OkObjectResult(cipherText);
        }
    }
}
