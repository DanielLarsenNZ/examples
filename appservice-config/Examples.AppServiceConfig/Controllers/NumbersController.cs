using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;

namespace Examples.AppServiceConfig.Controllers
{
    /// <summary>
    /// Returns a unique sequential number for every request.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class NumbersController : ControllerBase
    {
        // TODO: Configurable Seed numbers
        private const ulong SeedNumber = 1;
        private const string DefaultKeyName = "Default";

        // TODO: Use Cosmos DB as global store
        private static readonly ConcurrentDictionary<string, ulong> _mockNumbersData
            = new ConcurrentDictionary<string, ulong>(new[] { new KeyValuePair<string, ulong>(DefaultKeyName, 0) });

        // GET api/numbers[?key=sequenceName]
        [HttpGet]
        public ActionResult<dynamic> Get(string key = DefaultKeyName)
        {
            const int maxAttempts = 10;

            // first number shortcircuit
            if (_mockNumbersData.TryAdd(key, SeedNumber)) return Result(key, SeedNumber);

            for (int i = 0; i < maxAttempts; i++)
            {
                ulong lastNumber = _mockNumbersData[key];
                ulong newNumber = lastNumber + 1;
                if (_mockNumbersData.TryUpdate(key, lastNumber + 1, lastNumber)) return Result(key, newNumber);
            }

            return new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);
        }

        private dynamic Result(string key, ulong newNumber) => new { key, number = newNumber };
    }
}
