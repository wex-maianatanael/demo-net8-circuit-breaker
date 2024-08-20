using Demo.API._1.Clients.Contracts;
using Polly.CircuitBreaker;
using System.Net;

namespace Demo.API._1.Clients
{
    public class Api2Client : IApi2Client
    {
        private readonly ILogger<Api2Client> _logger;
        private readonly HttpClient _httpClient;

        public Api2Client(ILogger<Api2Client> logger, HttpClient client)
        {
            _logger = logger;
            this._httpClient = client;
        }

        public async Task<HttpResponseMessage> GetAsync()
        {
            var response = await _httpClient.GetAsync($"/api/samples2");

            if (!response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                _logger.LogError("Error on calling API 2. Status code: {StatusCode}", response.StatusCode);

                var logPrefix = GetType().Name;
                throw new BrokenCircuitException($"{logPrefix} | API 2 is inoperative due circuit-breaker opening.");
            }

            return response;
        }
    }
}
