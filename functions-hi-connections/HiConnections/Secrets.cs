using Microsoft.ApplicationInsights;
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
        protected TelemetryClient _telemetry;

        protected Secrets()
        {
            // Decrypt loop count can be overidden by App Setting "DecryptLoopCount"
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
                // HTTP Status 429
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
            // Group exceptions by Type
            var errorsGrouped = errorBag.GroupBy(e => e.GetType());

            // Log first exception of each type
            foreach (var ex in errorsGrouped) _telemetry.TrackException(ex.First());

            // return summary of errors and successful operations
            // Return HTTP status 500 if any errors
            return new JsonResult(
                           new
                           {
                               Errors = errorsGrouped.Select(g => $"{g.Count()} x {g.First().Message}"),
                               Decrypted = $"{bag.Count} x {bag.FirstOrDefault()}"
                           })
            { StatusCode = errorBag.Any() ? StatusCodes.Status500InternalServerError : StatusCodes.Status200OK };
        }

        protected abstract KeyVaultCrypto GetCrypto();

        protected async Task<IActionResult> GetSecrets()
        {
            // get some cipher text
            string cipherText = await Encrypt(GetCrypto(), "Plain text");

            // build a list of tasks
            var tasks = new List<Task>();
            for (int i = 0; i < _decryptLoopCount; i++)
            {
                tasks.Add(Decrypt(GetCrypto(), cipherText));
            }

            try
            {
                // execute tasks in parallel
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _telemetry?.TrackException(ex);
            }

            return ActionResult(_bag, _errorBag);
        }
    }
}
