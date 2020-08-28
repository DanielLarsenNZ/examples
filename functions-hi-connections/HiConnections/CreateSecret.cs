using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using static Microsoft.Azure.KeyVault.KeyVaultClient;

namespace HiConnections
{
    public class CreateSecret : Secrets
    {
        private readonly KeyVaultCrypto _crypto;
        private static readonly KeyVaultClient _keyVaultClient =
            new KeyVaultClient(new AuthenticationCallback(
                new AzureServiceTokenProvider().KeyVaultTokenCallback));

        public CreateSecret(TelemetryConfiguration telemetryConfiguration)
        {
            _telemetry = new TelemetryClient(telemetryConfiguration);
            _crypto = new KeyVaultCrypto(_keyVaultClient, Environment.GetEnvironmentVariable("KeyId"), _telemetry);
        }

        [FunctionName("CreateSecret")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var cipherText = await Encrypt(GetCrypto(), "Plain Text");

            return new OkObjectResult(cipherText);
        }

        protected override KeyVaultCrypto GetCrypto() => _crypto;
    }
}
