using Demo.API._1.Clients;
using Demo.API._1.Clients.Contracts;
using Demo.API._1.CustomMiddleware;
using Demo.API._1.HttpHandlers;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddLogging(logging => logging.AddConsole());

// v1 - it's working
var retryPolicy = GetRetryPolicy();
var circuitBreakerPolicy = GetCircuitBreakerPolicy();

builder.Services.AddTransient<CircuitBreakerExceptionHandler>();

builder.Services.AddHttpClient<IApi2Client, Api2Client>(
    client => {
        client.BaseAddress = new Uri(@"http://localhost:5081"); // it's using http protocol just to keep it simple
    })
    .SetHandlerLifetime(TimeSpan.FromMinutes(5))  // Sample: default lifetime is 2 minutes
    .AddHttpMessageHandler<CircuitBreakerExceptionHandler>()
    .AddPolicyHandler(retryPolicy)
    .AddPolicyHandler(circuitBreakerPolicy);

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}

static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(3, TimeSpan.FromSeconds(30));
}

/*// v2 - not working

logging.Services.AddHttpClient<IApi2Client, Api2Client>(
    client => {
        client.BaseAddress = new Uri(@"http://localhost:5081"); // it's using http protocol just to keep it simple
    })
    .AddHttpMessageHandler(() => new CircuitBreakerExceptionHandler())
    .AddTransientHttpErrorPolicy(policy =>
        policy.WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
            onRetry: (outcome, timespan, retryAttempt, context) =>
            {
                Console.WriteLine($"Retry {retryAttempt} failed. Waiting {timespan.Seconds} seconds before trying again.");
            })
        )
    .AddTransientHttpErrorPolicy(policy =>
        policy.CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 3,
            durationOfBreak: TimeSpan.FromSeconds(30))
        );
*/

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<CircuitBreakerExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
