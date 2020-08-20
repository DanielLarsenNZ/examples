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
    public class GetSecrets2 : Secrets
    {
        private readonly KeyVaultCrypto _crypto;
        private static readonly KeyVaultClient _keyVaultClient =
            new KeyVaultClient(new AuthenticationCallback(
                new AzureServiceTokenProvider().KeyVaultTokenCallback));

        public GetSecrets2(TelemetryConfiguration telemetryConfiguration) : base(telemetryConfiguration)
        {
            _crypto = new KeyVaultCrypto(_keyVaultClient, Environment.GetEnvironmentVariable("KeyId"));
        }

        [FunctionName("GetSecrets2")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation(nameof(GetSecrets2));
            return await GetSecrets();
        }

        protected override KeyVaultCrypto GetCrypto() => _crypto;
    }
}
