using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAdvert.Web.ServiceClient;

namespace WebAdvert.Web.Api
{
    [Route("api")]
    [ApiController]
    public class InternalApisController : ControllerBase
    {
        private readonly IAdvertApiClient _advertApiClient;

        public InternalApisController(IAdvertApiClient advertApiClient)
        {
            _advertApiClient = advertApiClient;
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetAsync(string id)
        {
            var record = await _advertApiClient.GetAsync(id).ConfigureAwait(false);
 
            return Ok(record);
        }
    }
}
