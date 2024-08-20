using Polly.CircuitBreaker;
using System.Net;

namespace Demo.API._1.HttpHandlers
{
    public class CircuitBreakerExceptionHandler : DelegatingHandler
    {
        private readonly ILogger<CircuitBreakerExceptionHandler> _logger;

        public CircuitBreakerExceptionHandler(ILogger<CircuitBreakerExceptionHandler> logger)
        {
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var logPrefix = GetType().Name;

            try
            {
                var response = await base.SendAsync(request, cancellationToken);

                _logger.LogInformation("{LogPrefix} | The circuit-breaker is closed. Let's keep moving.", logPrefix);

                return response;
            }
            catch (BrokenCircuitException)
            {
                _logger.LogCritical("{LogPrefix} | The circuit breaker is open. Dependent services are inoperative.", logPrefix);

                return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                {
                    Content = new StringContent("The service is inoperative, please try again later."),
                    ReasonPhrase = "Circuit-Breaker"
                };
            }
            catch(Exception ex)
            {
                _logger.LogCritical(ex, "{LogPrefix} | Something critical happened when sending an outgoing HTTP request.", logPrefix);
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }
    }
}
