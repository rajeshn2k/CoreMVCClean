using Core.Library.Clean.AdditionalService;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using System.Net;

namespace Core.MVC.Clean
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddHttpContextAccessor();

            // --------------------------------------------------
            // Book API
            // --------------------------------------------------

            services.AddHttpClient<BookDirector>(client =>
            {
                client.BaseAddress = new Uri(configuration["bookAPIUrl"]!);

                // Let Polly's timeout strategy control timeout.
                client.Timeout = Timeout.InfiniteTimeSpan;
            })
            .AddResilienceHandler("BookAPIResilience", (builder, context) =>
                {
                    var logger = context.ServiceProvider.GetRequiredService<ILogger<ResiliencePipelineBuilder>>();

                    /*
                     * Retry must be outermost to retry on circuit breaker failures
                     * Pipeline: Retry → CircuitBreaker → Timeout → HTTP Request
                     * This allows:
                     * - Each individual HTTP call to be tracked by CircuitBreaker
                     * - CircuitBreaker to break on individual timeouts (not just final retry result)
                     * - Retry to handle BrokenCircuitException if needed
                     */
                    builder.AddRetry(
                       new RetryStrategyOptions<HttpResponseMessage>
                       {
                           MaxRetryAttempts = 3,
                           Delay = TimeSpan.FromSeconds(2),
                           BackoffType = DelayBackoffType.Exponential,
                           //1. Exception test retries because its explicitly handle by HttpRequestException
                           //2. Timeout  test retries because its explicitly handle by TimeoutRejectedException
                           //3. CircuitBreaker failures should also be retried if desired
                           ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                                               .Handle<HttpRequestException>()
                                               .Handle<TimeoutRejectedException>()
                                               .HandleResult(response =>
                                                   response.StatusCode == HttpStatusCode.RequestTimeout ||
                                                   response.StatusCode == HttpStatusCode.BadGateway ||
                                                   response.StatusCode == HttpStatusCode.ServiceUnavailable ||
                                                   response.StatusCode == HttpStatusCode.GatewayTimeout),
                           OnRetry = args =>
                           {
                               logger.LogWarning(
                                    "Retry {RetryAttempt} for {ServiceName}. Delay={Delay}. StatusCode={StatusCode}. Exception={Exception}",
                                    args.AttemptNumber, "BookAPI", args.RetryDelay, args.Outcome.Result?.StatusCode, args.Outcome.Exception);

                               return default;
                           }
                       });

                    /*
                     * CircuitBreaker tracks individual HTTP call failures (including each timeout)
                     * With Retry outermost, CircuitBreaker sees each timeout attempt, not just final result
                     * MVC → Retry → CircuitBreaker → Timeout → HTTP Request
                     */
                    builder.AddCircuitBreaker(
                        new CircuitBreakerStrategyOptions<HttpResponseMessage>
                        {
                            FailureRatio = 0.5,
                            MinimumThroughput = 3,
                            SamplingDuration = TimeSpan.FromSeconds(30), // Reduced from 1 minute to 30 seconds
                            BreakDuration = TimeSpan.Parse("00:00:30"),
                            ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                                    .Handle<HttpRequestException>()
                                    .Handle<TimeoutRejectedException>()
                                    .HandleResult(response => !response.IsSuccessStatusCode),
                            OnOpened = args =>
                            {
                                logger.LogWarning(
                                    "Circuit opened for {ServiceName}. BreakDuration={BreakDuration}. StatusCode={StatusCode}. Exception={Exception}",
                                    "BookAPI", TimeSpan.Parse("00:00:30"), args.Outcome.Result?.StatusCode, args.Outcome.Exception);

                                return default;
                            },

                            OnClosed = args =>
                            {
                                logger.LogInformation("Circuit closed for {ServiceName}", "BookAPI");
                                return default;
                            },

                            OnHalfOpened = args =>
                            {
                                logger.LogInformation("Circuit half-open for {ServiceName}", "BookAPI");
                                return default;
                            }
                        });

                    /*
                     * Testing HTTPClinet for Book API Request due to delibrate time out
                     * HOW TO - API Shuld take more time like 25 Secs and Polly should not wait more than 5 secons
                     * REST Timeout to 45 Seconds for normal flow or Polly to capture exception
                     * Timeout must be innermost to apply to each individual request attempt
                     */
                    builder.AddTimeout(new TimeoutStrategyOptions
                    {
                        //Timeout = TimeSpan.Parse("00:02:00"),//45 Secs or 2 Mins etc.
                        Timeout = TimeSpan.FromSeconds(5),//5 Secs
                        OnTimeout = args =>
                        {
                            logger.LogWarning("Timeout for {ServiceName}. Timeout={Timeout}", "BookAPI", args.Timeout);
                            return default;
                        }
                    });
                });

            // --------------------------------------------------
            // Person API
            // --------------------------------------------------

            services.AddHttpClient<PersonDirector>(client =>
            {
                client.BaseAddress = new Uri(configuration["personAPIUrl"]!);

                // Let Polly's timeout strategy control timeout.
                client.Timeout = Timeout.InfiniteTimeSpan;
            });
            //DONT CONSIDER NOT REQUIRED NOW WILL BE REMOVED THIS MODULE
            /*
            .AddResilienceHandler("PersonAPIResilience", (builder, context) =>
            {
                var logger = context.ServiceProvider.GetRequiredService<ILogger<ResiliencePipelineBuilder>>();

                builder.AddTimeout(new TimeoutStrategyOptions
                {
                    Timeout = TimeSpan.Parse("00:00:30"),

                    OnTimeout = args =>
                    {
                        logger.LogWarning("Timeout for {ServiceName}. Timeout={Timeout}", "BookAPI", args.Timeout);
                        return default;
                    }
                });

                builder.AddRetry(
                   new RetryStrategyOptions<HttpResponseMessage>
                   {
                       MaxRetryAttempts = 5,
                       Delay = TimeSpan.Parse("00:00:02"),
                       BackoffType = DelayBackoffType.Exponential,
                       ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                                           .Handle<HttpRequestException>()
                                           .HandleResult(response =>
                                               response.StatusCode == HttpStatusCode.RequestTimeout ||
                                               response.StatusCode == HttpStatusCode.BadGateway ||
                                               response.StatusCode == HttpStatusCode.ServiceUnavailable ||
                                               response.StatusCode == HttpStatusCode.GatewayTimeout),
                       OnRetry = args =>
                       {
                           logger.LogWarning(
                                "Retry {RetryAttempt} for {ServiceName}. Delay={Delay}. StatusCode={StatusCode}. Exception={Exception}",
                                args.AttemptNumber, "BookAPI", args.RetryDelay, args.Outcome.Result?.StatusCode, args.Outcome.Exception);

                           return default;
                       }
                   });

                builder.AddCircuitBreaker(
                    new CircuitBreakerStrategyOptions<HttpResponseMessage>
                    {
                        FailureRatio = 1.0,
                        MinimumThroughput = 5,
                        SamplingDuration = TimeSpan.FromSeconds(10),
                        BreakDuration = TimeSpan.Parse("00:00:30"),
                        ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                                .Handle<HttpRequestException>()
                                .HandleResult(response => !response.IsSuccessStatusCode),
                        OnOpened = args =>
                        {
                            logger.LogWarning(
                                "Circuit opened for {ServiceName}. BreakDuration={BreakDuration}. StatusCode={StatusCode}. Exception={Exception}",
                                "BookAPI", TimeSpan.Parse("00:00:30"), args.Outcome.Result?.StatusCode, args.Outcome.Exception);

                            return default;
                        },

                        OnClosed = args =>
                        {
                            logger.LogInformation("Circuit closed for {ServiceName}", "BookAPI");
                            return default;
                        },

                        OnHalfOpened = args =>
                        {
                            logger.LogInformation("Circuit half-open for {ServiceName}", "BookAPI");
                            return default;
                        }
                    });
            });
            */

            return services;
        }
    }
}