using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using System.Net.Http;

namespace Core.Library.Clean.AdditionalService.Resilience
{
    /// <summary>
    /// Factory for creating circuit breaker policies for external API calls
    /// </summary>
    public class CircuitBreakerPolicyFactory
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<CircuitBreakerPolicyFactory> _logger;

        public CircuitBreakerPolicyFactory(
            IConfiguration configuration,
            ILogger<CircuitBreakerPolicyFactory> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Creates a circuit breaker policy for the specified service
        /// </summary>
        public IAsyncPolicy<HttpResponseMessage> CreateCircuitBreakerPolicy(string serviceName)
        {
            var config = _configuration.GetSection($"CircuitBreaker:{serviceName}");
            
            if (!config.GetValue<bool>("Enabled", true))
            {
                _logger.LogInformation("Circuit breaker disabled for {ServiceName}", serviceName);
                return Policy.NoOpAsync<HttpResponseMessage>();
            }

            var exceptionsAllowedBeforeBreaking = config.GetValue<int>("ExceptionsAllowedBeforeBreaking", 5);
            var durationOfBreak = config.GetValue<TimeSpan>("BreakDuration", TimeSpan.FromSeconds(30));

            _logger.LogInformation(
                "Creating circuit breaker policy for {ServiceName}: " +
                "ExceptionsAllowedBeforeBreaking={ExceptionsAllowed}, BreakDuration={BreakDuration}",
                serviceName, exceptionsAllowedBeforeBreaking, durationOfBreak);

            return Policy
                .Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .CircuitBreakerAsync(
                    exceptionsAllowedBeforeBreaking: exceptionsAllowedBeforeBreaking,
                    durationOfBreak: durationOfBreak,
                    onBreak: (exception, breakDelay) =>
                    {
                        _logger.LogWarning(
                            "Circuit broken for {ServiceName}. Delay: {Delay}s. Exception: {Exception}",
                            serviceName, breakDelay.TotalSeconds, exception);
                    },
                    onReset: () =>
                    {
                        _logger.LogInformation("Circuit reset for {ServiceName}", serviceName);
                    },
                    onHalfOpen: () =>
                    {
                        _logger.LogInformation("Circuit half-open for {ServiceName}", serviceName);
                    });
        }

        /// <summary>
        /// Creates a retry policy for the specified service
        /// </summary>
        public IAsyncPolicy<HttpResponseMessage> CreateRetryPolicy(string serviceName)
        {
            var config = _configuration.GetSection("CircuitBreaker:Retry");
            var maxRetries = config.GetValue<int>("MaxRetries", 3);
            var retryDelay = config.GetValue<TimeSpan>("RetryDelay", TimeSpan.FromSeconds(2));
            var exponentialBackoff = config.GetValue<bool>("ExponentialBackoff", true);

            _logger.LogInformation(
                "Creating retry policy for {ServiceName}: " +
                "MaxRetries={MaxRetries}, RetryDelay={RetryDelay}s, ExponentialBackoff={ExponentialBackoff}",
                serviceName, maxRetries, retryDelay.TotalSeconds, exponentialBackoff);

            var retryPolicy = Policy
                .Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r => 
                    r.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable ||
                    r.StatusCode == System.Net.HttpStatusCode.GatewayTimeout ||
                    r.StatusCode == System.Net.HttpStatusCode.RequestTimeout)
                .WaitAndRetryAsync(
                    retryCount: maxRetries,
                    sleepDurationProvider: retryAttempt => 
                        exponentialBackoff 
                            ? TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) * retryDelay
                            : retryDelay,
                    onRetry: (outcome, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(
                            "Retry {RetryCount} for {ServiceName} after {Delay}s. Result: {Result}",
                            retryCount, serviceName, timeSpan.TotalSeconds, outcome.Result?.StatusCode);
                    });

            return retryPolicy;
        }

        /// <summary>
        /// Creates a timeout policy for the specified service
        /// </summary>
        public IAsyncPolicy<HttpResponseMessage> CreateTimeoutPolicy(string serviceName)
        {
            var config = _configuration.GetSection("CircuitBreaker:Timeout");
            var timeout = config.GetValue<TimeSpan>("DefaultTimeout", TimeSpan.FromSeconds(30));

            _logger.LogInformation(
                "Creating timeout policy for {ServiceName}: Timeout={Timeout}s",
                serviceName, timeout.TotalSeconds);

            return Policy.TimeoutAsync<HttpResponseMessage>(timeout);
        }
    }
}