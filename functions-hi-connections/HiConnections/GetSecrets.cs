using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Microsoft.Azure.KeyVault.KeyVaultClient;

namespace HiConnections
{

    public class GetSecrets
    {
        private readonly ConcurrentBag<string> _bag = new ConcurrentBag<string>();
        private readonly ConcurrentBag<Exception> _errorBag = new ConcurrentBag<Exception>();
        private readonly int _decryptLoopCount = 30;

        public GetSecrets()
        {
            if (int.TryParse(Environment.GetEnvironmentVariable("DecryptLoopCount"), out int count)) _decryptLoopCount = count;
        }

        [FunctionName("GetSecrets")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var tasks = new List<Task>();

            for (int i = 0; i < _decryptLoopCount; i++)
            {
                tasks.Add(Decrypt("Cid22qFp/J+3gGavX5TmWb49ZHto6+8ZTJOz8h7umjiqfv8gqt/2QrDd/2ExZ5+Fw7javD6Oy2Stu2JGSA7sUk/2UrbOHOeMhCUOvfDbrBAblAHbofgq/cXsRi2aCgeoda0lYZKm5+4WXJkOVzbCppGV03/lcZ/H2IED8xzm27nWlzYuIRCle2TJNZNyjYxgtfP6eggDwJ3Fcb0wwKJcBsyP2RMQi9TsYCACnAniT9IAJMnd3cn6sboo48OLgcxvD89uMLjE0FLVNvFrvct+E/O/IKFoypcVf/6urz32eYwLq1Qeh1oEcIKh6drnMqlm3IGEV0rlAMcGyAtD0/A1PfXGMJ5N/2g/uSO06ZR7+DybJWNpFx8KrhLuCQhMcDqWZHSwHpQUpWUIts9RLnklz/W+f1CsgCb+nWwtOLR/u47bUYv/ljO5ue37r3qE4otQto4744wAYYtcGwlE23NKpoax6rK1EOOgE9u824LVm6aun5QuXZhuVl6g/9+FpLSbb3XskInQKmc6pc4P1r5ZmX3iVRuMXVxMWni61kPegkDA2maeUSt6zyRK1kjeBfT8Z/CmQQsTKWpxHKPCTFw1omMMCbJpzhVTCwvfmFbJvnZiCNeGTEiSBkb+Diy8b0Mtbby175fP3Z0rHlWmvmK32prFN+ywmvGYQnRKxhUzMfo="));
            }

            await Task.WhenAll(tasks);
            return GetSecretsHelper.ActionResult(_bag, _errorBag);
        }

        private async Task Decrypt(string cipherText)
        {
            var keyVaultClient = new KeyVaultClient(
                new AuthenticationCallback(new AzureServiceTokenProvider().KeyVaultTokenCallback));
            var crypto = new KeyVaultCrypto(keyVaultClient, "https://helloase-aue-kv.vault.azure.net/keys/key1");
            try
            {
                _bag.Add(await crypto.DecryptAsync(cipherText));
            }
            catch (Exception ex)
            {
                _errorBag.Add(ex);
            }
        }
    }
}
