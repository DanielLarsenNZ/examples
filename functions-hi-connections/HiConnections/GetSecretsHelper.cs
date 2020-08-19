using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace HiConnections
{
    internal static class GetSecretsHelper
    {
        internal static IActionResult ActionResult(ConcurrentBag<string> bag, ConcurrentBag<Exception> errorBag)
        {
            if (errorBag.Any())
            {
                return new JsonResult(errorBag.First().Message) { StatusCode = StatusCodes.Status500InternalServerError };
            }

            return new JsonResult(bag.ToArray());
        }
    }
}
