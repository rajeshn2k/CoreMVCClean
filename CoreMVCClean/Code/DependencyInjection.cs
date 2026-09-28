using Core.Library.Clean.AdditionalService;
using Core.Library.Clean.AdditionalService.Resilience;
using Microsoft.Extensions.DependencyInjection;
using Polly;

namespace Core.MVC.Clean
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddSingleton(configuration);
            services.AddHttpContextAccessor();
            
            // Register circuit breaker policy factory
            services.AddSingleton<CircuitBreakerPolicyFactory>();

            // --------------------------------------------------
            // The URL is configured in appsettings.json
            // --------------------------------------------------

            var bookAPIUrl = configuration["bookAPIUrl"];
            var personAPIUrl = configuration["personAPIUrl"];

            // Create policies (created before service provider is available)
            var circuitBreakerFactory = new CircuitBreakerPolicyFactory(
                configuration,
                new Microsoft.Extensions.Logging.LoggerFactory().CreateLogger<CircuitBreakerPolicyFactory>());

            var bookCircuitBreakerPolicy = circuitBreakerFactory.CreateCircuitBreakerPolicy("BookAPI");
            var bookRetryPolicy = circuitBreakerFactory.CreateRetryPolicy("BookAPI");
            var bookTimeoutPolicy = circuitBreakerFactory.CreateTimeoutPolicy("BookAPI");

            var personCircuitBreakerPolicy = circuitBreakerFactory.CreateCircuitBreakerPolicy("PersonAPI");
            var personRetryPolicy = circuitBreakerFactory.CreateRetryPolicy("PersonAPI");
            var personTimeoutPolicy = circuitBreakerFactory.CreateTimeoutPolicy("PersonAPI");

            // --------------------------------------------------
            // Register the API services used as dependencies by the MCP tools.
            // With Polly circuit breaker integration
            // --------------------------------------------------

            services.AddHttpClient<BookDirector>(client =>
            {
                client.BaseAddress = new Uri(bookAPIUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddPolicyHandler(bookCircuitBreakerPolicy)
            .AddPolicyHandler(bookRetryPolicy)
            .AddPolicyHandler(bookTimeoutPolicy);

            services.AddHttpClient<PersonDirector>(client =>
            {
                client.BaseAddress = new Uri(personAPIUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddPolicyHandler(personCircuitBreakerPolicy)
            .AddPolicyHandler(personRetryPolicy)
            .AddPolicyHandler(personTimeoutPolicy);

            return services;
        }
    }
}
