using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;
using static Microsoft.Azure.KeyVault.KeyVaultClient;

namespace HiConnections
{
    public class GetSecrets : Secrets
    {
        private readonly KeyVaultCrypto _crypto;
        private static HttpClient _http = new HttpClient();
        private static readonly KeyVaultClient _keyVaultClient =
            new KeyVaultClient(new AuthenticationCallback(
                new AzureServiceTokenProvider().KeyVaultTokenCallback),
                _http);

        public GetSecrets(TelemetryConfiguration telemetryConfiguration) : base(telemetryConfiguration)
        {
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
            var keyVaultClient = new KeyVaultClient(
                new AuthenticationCallback(new AzureServiceTokenProvider().KeyVaultTokenCallback));
            return new KeyVaultCrypto(keyVaultClient, "https://helloase-aue-kv.vault.azure.net/keys/key1");
        }
    }
}
