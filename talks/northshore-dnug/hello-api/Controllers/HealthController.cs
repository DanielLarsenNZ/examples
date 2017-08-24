using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace hello_api.Controllers
{
    [Route("api/[controller]")]
    public class HealthController : Controller
    {
        // GET api/values
        [HttpGet]
        public dynamic Get()
        {
            return new { Status = "Ok", Version = "1.0.0", Now = DateTime.Now };
        }
    }
}
