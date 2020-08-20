using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Rest;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HiConnections
{
    public abstract class Secrets
    {
        protected readonly ConcurrentBag<string> _bag = new ConcurrentBag<string>();
        protected readonly ConcurrentBag<Exception> _errorBag = new ConcurrentBag<Exception>();
        private readonly int _decryptLoopCount = 30;
        protected readonly TelemetryClient _telemetry;

        protected Secrets(TelemetryConfiguration telemetryConfiguration)
        {
            _telemetry = new TelemetryClient(telemetryConfiguration);
            if (int.TryParse(Environment.GetEnvironmentVariable("DecryptLoopCount"), out int count)) _decryptLoopCount = count;
        }

        protected async Task Decrypt(KeyVaultCrypto crypto, string cipherText)
        {
            try
            {
                _bag.Add(await crypto.DecryptAsync(cipherText));
            }
            catch (HttpOperationException ex) when (ex.Response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                _errorBag.Add(ex);
            }
            catch (Exception ex)
            {
                _errorBag.Add(ex);
            }
        }

        protected async Task<string> Encrypt(KeyVaultCrypto crypto, string plainText)
            => await crypto.EncryptAsync(plainText);

        protected IActionResult ActionResult(ConcurrentBag<string> bag, ConcurrentBag<Exception> errorBag)
        {
            var errorsGrouped = errorBag.GroupBy(e => e.GetType());
            foreach (var ex in errorsGrouped) _telemetry.TrackException(ex.First());

            return new JsonResult(
                           new
                           {
                               Errors = errorsGrouped.Select(g => $"{g.Count()} x {g.First().Message}"),
                               Results = $"{bag.Count} x {bag.FirstOrDefault()}"
                           })
            { StatusCode = errorBag.Any() ? StatusCodes.Status500InternalServerError : StatusCodes.Status200OK };
        }

        protected abstract KeyVaultCrypto GetCrypto();

        protected async Task<IActionResult> GetSecrets()
        {
            string cipherText = await Encrypt(GetCrypto(), "Plain text");

            var tasks = new List<Task>();
            for (int i = 0; i < _decryptLoopCount; i++)
            {
                tasks.Add(Decrypt(GetCrypto(), cipherText));
            }

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _telemetry.TrackException(ex);
            }

            return ActionResult(_bag, _errorBag);
        }
    }
}
