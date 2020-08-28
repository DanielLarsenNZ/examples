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
using System.Net.Http;
using System.Threading.Tasks;
using static Microsoft.Azure.KeyVault.KeyVaultClient;

namespace HiConnections
{
    public class GetSecrets3 : Secrets
    {
        private readonly KeyVaultCrypto _crypto;
        private static HttpClient _http = new HttpClient();
        private static readonly KeyVaultClient _keyVaultClient =
            new KeyVaultClient(new AuthenticationCallback(
                new AzureServiceTokenProvider().KeyVaultTokenCallback),
                _http);

        public GetSecrets3(TelemetryConfiguration telemetryConfiguration)
        {
            _telemetry = new TelemetryClient(telemetryConfiguration);
            _crypto = new KeyVaultCrypto(_keyVaultClient, Environment.GetEnvironmentVariable("KeyId"), _telemetry);
        }

        /// <summary>
        /// This version uses static KeyVaultClient with static HttpClient
        /// </summary>
        [FunctionName("GetSecrets3")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation(nameof(GetSecrets3));
            return await GetSecrets();
        }

        protected override KeyVaultCrypto GetCrypto() => _crypto;
    }
}
