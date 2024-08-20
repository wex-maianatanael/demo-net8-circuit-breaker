using Microsoft.AspNetCore.Mvc;
using Polly.CircuitBreaker;

namespace Demo.API._1.CustomMiddleware
{
    public class CircuitBreakerExceptionMiddleware
	{
		private readonly RequestDelegate _next;
        private readonly ILogger<CircuitBreakerExceptionMiddleware> _logger;

        public CircuitBreakerExceptionMiddleware(RequestDelegate next, ILogger<CircuitBreakerExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var logPrefix = GetType().Name;

            try
            {
                await _next(context);
            }
            catch (BrokenCircuitException ex)
            {
                if (ex.Message.Contains("circuit-breaker"))
                {
                    _logger.LogCritical(ex, "{LogPrefix} | {ErrorMessage}.", logPrefix, ex.Message);
                    var response = new StatusCodeResult(503);
                    await response.ExecuteResultAsync(new ActionContext()
                    {
                        HttpContext = context,
                        RouteData = new RouteData(context.GetRouteData())
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "{LogPrefix} | Fatal error occurred in the application.", logPrefix);
                var response = new StatusCodeResult(500);
                await response.ExecuteResultAsync(new ActionContext()
                {
                    HttpContext = context,
                    RouteData = new RouteData(context.GetRouteData())
                });
            }
        }
    }
}
