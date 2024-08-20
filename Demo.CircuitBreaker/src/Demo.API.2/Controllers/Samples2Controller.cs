using Microsoft.AspNetCore.Mvc;

namespace Demo.API._2.Controllers
{
    [ApiController]
    [Route("api/samples2")]
    public class Samples2Controller : ControllerBase
    {
        private readonly ILogger<Samples2Controller> _logger;

        public Samples2Controller(ILogger<Samples2Controller> logger)
        {
            _logger = logger;
        }

        [HttpGet()]
        public async Task<IActionResult> GetAsync()
        {
            try
            {
                // comment this task to see the circuit-breaker closing once it's opened.
                await Task.Run(() =>
                {
                    throw new Exception("Exception from Samples2Controller");
                });

                return Ok("Samples 2");
            }
            catch (Exception ex)
            {
                _logger.LogCritical("Something critical happened.");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
