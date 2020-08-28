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
    public class GetSecrets : Secrets
    {
        public GetSecrets(TelemetryConfiguration telemetryConfiguration)
        {
            _telemetry = new TelemetryClient(telemetryConfiguration);
        }

        /// <summary>
        /// This version news up a KeyVaultClient for every decryption
        /// </summary>
        [FunctionName("GetSecrets")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation(nameof(GetSecrets));
            return await GetSecrets();
        }

        protected override KeyVaultCrypto GetCrypto()
        {
            return new KeyVaultCrypto(
                new KeyVaultClient(new AuthenticationCallback(new AzureServiceTokenProvider().KeyVaultTokenCallback)),
                Environment.GetEnvironmentVariable("KeyId"),
                _telemetry);
        }
    }
}
