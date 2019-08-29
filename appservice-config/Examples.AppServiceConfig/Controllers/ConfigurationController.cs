using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Examples.AppServiceConfig.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigurationController : ControllerBase
    {
        readonly IConfiguration _config;

        public ConfigurationController(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Get config from various sources
        /// </summary>
        /// <remarks>This is a public endpoint. For an example of an authenticated endpoint see https://github.com/DanielLarsenNZ/rdostr/blob/master/src/Rdostr/Rdostr.Configuration/Controllers/ConfigurationController.cs </remarks>
        [HttpGet]
        public ActionResult Get()
        {
            var result = new List<KeyValuePair<string, string>>();

            // Add the value of a specific Key Vault key named "AppServiceConfig--DataKey1" to the result
            // Note: This value is retrieved from the Configuration Store, not from KeyVault. The Configuration 
            // store was loaded at startup.
            result.Add(new KeyValuePair<string, string>("AppServiceConfig:Secret1", _config["AppServiceConfig:Secret1"]));

            return new JsonResult(result.ToArray());
        }
    }
}