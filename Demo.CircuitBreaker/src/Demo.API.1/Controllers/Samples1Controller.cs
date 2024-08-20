using Demo.API._1.Clients.Contracts;
using Microsoft.AspNetCore.Mvc;
using Polly.CircuitBreaker;
using System.Net;

namespace Demo.API._1.Controllers
{
    [ApiController]
    [Route("api/samples1")]
    public class Samples1Controller : ControllerBase
    {
        private readonly ILogger<Samples1Controller> _logger;
        private readonly IApi2Client _api2Client;

        public Samples1Controller(ILogger<Samples1Controller> logger, IApi2Client api2Client)
        {
            _logger = logger;
            _api2Client = api2Client;
        }

        [HttpGet()]
        public async Task<IActionResult> GetAsync()
        {
            var logPrefix = GetType().Name;

            try
            {
                var response = await _api2Client.GetAsync();

                return response.StatusCode switch
                {
                    HttpStatusCode.NotFound => NotFound(),
                    HttpStatusCode.BadRequest => BadRequest(),
                    _ => Ok(await response.Content.ReadAsStringAsync()),
                };
            }
            catch (BrokenCircuitException ex)
            {
                _logger.LogCritical(ex, "{LogPrefix} | The circuit breaker is open. Dependent services are inoperative.", logPrefix);
                return StatusCode((int)HttpStatusCode.ServiceUnavailable, ex.Message);
                //throw; // uncomment this line so the custom middleware can handled the exception
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
