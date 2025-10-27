using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.PollyPolicies;

public class UsersMicroservicePolicies : IUsersMicroservicePolicies
{
    private readonly ILogger<UsersMicroservicePolicies> _logger;

    public UsersMicroservicePolicies(ILogger<UsersMicroservicePolicies> logger)
    {
        _logger = logger;
    }
    
    public IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        AsyncRetryPolicy<HttpResponseMessage> retryPolicy = Policy.HandleResult<HttpResponseMessage>(result => !result.IsSuccessStatusCode)
            .WaitAndRetryAsync(
                    retryCount: 5,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (result, timeSpan, retryCount, context) =>
                    {
                        _logger.LogInformation($"Retry {retryCount} after {timeSpan.TotalSeconds} seconds");
                    });

        return retryPolicy;
    }

    public IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        AsyncCircuitBreakerPolicy<HttpResponseMessage> circuitBreakerPolicy = Policy.HandleResult<HttpResponseMessage>(result => !result.IsSuccessStatusCode)
            .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 3,
                    durationOfBreak: TimeSpan.FromMinutes(2),
                    // From open state to half open
                    onBreak: (result, timeSpan) =>
                    {
                        _logger.LogInformation($"Circuit breaker opened for {timeSpan.TotalMinutes} minutes due to consecutive 3 failures. The subsequent requests will be blocked");
                    },
                    // From half open state to closed state
                    onReset: () =>
                    {
                        _logger.LogInformation($"Circuit breaker closed. The subsequent requests will be allowed");
                    }
                );

        return circuitBreakerPolicy;
    }
}