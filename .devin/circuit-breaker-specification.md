# Circuit Breaker Implementation Specification

## Overview
This specification documents the implementation of circuit breaker functionality for API calls from the Person and Book Director modules. The circuit breaker pattern will protect the application from cascading failures and improve system resilience when external APIs are unavailable or experiencing issues.

**Document Version**: 1.0  
**Creation Date**: 2026-09-28  
**Status**: Requirements Analysis Complete

## Current State Analysis

### Existing Implementation

**Architecture**: The application uses a Director pattern with HttpClient for external API communication:

```
Controllers → Directors (BookDirector/PersonDirector) → External APIs
                   ↓
            BaseDirector (shared functionality)
                   ↓
            HttpClient with correlation ID support
```

**Current Components**:
- `BookDirector` - Handles Book entity API calls
- `PersonDirector` - Handles Person entity API calls  
- `BaseDirector` - Shared functionality (correlation ID, response handling)
- `DependencyInjection.cs` - HttpClient configuration with 5-minute timeout

**Current API Call Pattern**:
```csharp
// Example from BookDirector
public async Task<BookDTO> GetEntityByIdAsync(string entityId, CancellationToken cancellationToken)
{
    AddCorrelationIdHeader();
    var requestUrl = $"{entityId}";
    
    using var response = await httpClient.GetAsync(requestUrl, cancellationToken)
        .ConfigureAwait(false);
    
    return await HandleResponseAsync<BookDTO>(response, cancellationToken);
}
```

**Current Error Handling**:
- Standardized error responses with `ApiException`
- Error code mapping and correlation ID integration
- No retry logic or circuit breaking
- Direct HTTP calls without resilience patterns

### Limitations of Current Implementation

1. **No Circuit Breaking**: API failures can cascade through the system
2. **No Retry Logic**: Transient failures cause immediate errors
3. **No Fallback Mechanism**: No alternative behavior when APIs are down
4. **No Health Monitoring**: Cannot detect API degradation proactively
5. **No Timeout Control**: Only global 5-minute timeout (too long for many scenarios)
6. **No Bulkhead Isolation**: Failures in one API can affect others
7. **No Rate Limiting**: Could overwhelm external APIs with retries

## Circuit Breaker Requirements

### Functional Requirements

1. **Circuit State Management**
   - **Closed State**: Normal operation, requests pass through
   - **Open State**: Circuit is tripped, requests fail fast
   - **Half-Open State**: Testing if API has recovered

2. **Failure Detection**
   - Detect consecutive failures (configurable threshold)
   - Detect error rate exceeding threshold (percentage-based)
   - Detect timeout failures
   - Detect specific HTTP status codes (5xx, 503, 504)

3. **Automatic Recovery**
   - Transition from Open to Half-Open after timeout period
   - Test API health in Half-Open state
   - Return to Closed if health check succeeds
   - Return to Open if health check fails

4. **Fallback Mechanisms**
   - Return cached data when available
   - Return default values for non-critical operations
   - Return graceful error messages
   - Circuit open exceptions with clear messaging

5. **Monitoring and Logging**
   - Log circuit state transitions
   - Log failure counts and rates
   - Log circuit breaker metrics
   - Integrate with correlation ID system

### Non-Functional Requirements

1. **Performance**
   - Minimal overhead (<5% latency increase)
   - Fast failure detection (<100ms)
   - Efficient state management
   - Low memory footprint

2. **Reliability**
   - Prevent cascading failures
   - Protect system resources
   - Maintain system availability
   - Graceful degradation

3. **Configurability**
   - Per-service circuit breaker configuration
   - Environment-specific settings
   - Runtime configuration updates
   - Feature flags for enable/disable

4. **Observability**
   - Circuit state visibility
   - Health check endpoints
   - Metrics and monitoring
   - Alerting capabilities

## Implementation Options Analysis

### Option 1: Polly (Recommended)

**Description**: Polly is the most popular and mature .NET resilience library with comprehensive circuit breaker implementation.

**Pros**:
- ✅ Industry standard for .NET resilience patterns
- ✅ Comprehensive feature set (circuit breaker, retry, timeout, fallback, etc.)
- ✅ Excellent documentation and community support
- ✅ Integration with HttpClient via `AddHttpClient` extensions
- ✅ Supports multiple circuit breaker strategies
- ✅ Built-in metrics and monitoring
- ✅ Active development and maintenance
- ✅ Microsoft-recommended for .NET applications

**Cons**:
- ❌ Additional package dependency
- ❌ Learning curve for advanced features
- ❌ May be overkill for simple use cases

**Implementation Complexity**: Medium  
**Package**: `Polly`, `Polly.Extensions.Http`  
**Integration**: Native HttpClient integration via `AddHttpClient`

**Example Configuration**:
```csharp
services.AddHttpClient<BookDirector>(client =>
{
    client.BaseAddress = new Uri(bookAPIUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddPolicyHandler(GetCircuitBreakerPolicy())
.AddPolicyHandler(GetRetryPolicy());
```

**Recommended For**: Production systems requiring robust resilience patterns

---

### Option 2: Microsoft.Extensions.Http.Resilience

**Description**: Built-in .NET resilience extensions that provide standardized resilience patterns for HttpClient.

**Pros**:
- ✅ Official Microsoft package
- ✅ No third-party dependencies
- ✅ Native .NET integration
- ✅ Consistent with Microsoft patterns
- ✅ Lightweight and focused
- ✅ Built-in configuration support

**Cons**:
- ❌ Newer than Polly (less mature)
- ❌ Fewer features than Polly
- ❌ Less community support
- ❌ Limited advanced configurations
- ❌ Smaller ecosystem of extensions

**Implementation Complexity**: Low-Medium  
**Package**: `Microsoft.Extensions.Http.Resilience`  
**Integration**: Native `AddResilienceHandler` extension

**Example Configuration**:
```csharp
services.AddHttpClient<BookDirector>(client =>
{
    client.BaseAddress = new Uri(bookAPIUrl);
})
.AddResilienceHandler("BookAPI", builder =>
{
    builder.AddCircuitBreaker(new CircuitBreakerStrategyOptions
    {
        FailureRatio = 0.1,
        SamplingDuration = TimeSpan.FromSeconds(30),
        MinimumThroughput = 3,
        BreakDuration = TimeSpan.FromSeconds(30)
    });
});
```

**Recommended For**: Teams preferring Microsoft-native solutions with basic resilience needs

---

### Option 3: Custom Circuit Breaker Implementation

**Description**: Build a custom circuit breaker implementation from scratch using state machine pattern.

**Pros**:
- ✅ Full control over implementation
- ✅ No external dependencies
- ✅ Customized to specific requirements
- ✅ Learning opportunity for team
- ✅ Lightweight implementation

**Cons**:
- ❌ High development effort
- ❌ Maintenance burden
- ❌ Potential for bugs in edge cases
- ❌ Limited features compared to established libraries
- ❌ No community support or battle-testing
- ❌ Re-inventing the wheel

**Implementation Complexity**: High  
**Package**: None (custom implementation)  
**Integration**: Manual wrapping of HttpClient calls

**Example Structure**:
```csharp
public class CircuitBreaker
{
    private CircuitState _state = CircuitState.Closed;
    private int _failureCount = 0;
    private DateTime _lastFailureTime;
    
    public async Task<T> ExecuteAsync<T>(Func<Task<T>> action)
    {
        if (_state == CircuitState.Open)
        {
            if (DateTime.UtcNow - _lastFailureTime > _resetTimeout)
            {
                _state = CircuitState.HalfOpen;
            }
            else
            {
                throw new CircuitBreakerOpenException();
            }
        }
        
        try
        {
            var result = await action();
            Reset();
            return result;
        }
        catch (Exception ex)
        {
            RecordFailure();
            throw;
        }
    }
}
```

**Recommended For**: Educational purposes or highly specialized requirements not met by existing libraries

---

### Option 4: Steeltoe Resilience

**Description**: Steeltoe provides .NET resilience patterns with focus on cloud-native applications and microservices.

**Pros**:
- ✅ Cloud-native focus
- ✅ Integration with Spring Cloud ecosystem
- ✅ Good for microservices architectures
- ✅ Comprehensive resilience patterns
- ✅ Configuration management integration

**Cons**:
- ❌ Less popular than Polly
- ❌ More complex setup
- ❌ Heavier dependency
- ❌ More focused on cloud scenarios
- ❌ Steeper learning curve
- ❌ Less .NET-specific optimization

**Implementation Complexity**: Medium-High  
**Package**: `Steeltoe.Common.Http`  
**Integration**: Requires Steeltoe configuration system

**Example Configuration**:
```csharp
services.AddHttpClient<BookDirector>(client =>
{
    client.BaseAddress = new Uri(bookAPIUrl);
})
.AddCircuitBreakerHandler();
```

**Recommended For**: Cloud-native microservices using Spring Cloud ecosystem

---

## Implementation Recommendation

### Recommended Approach: **Option 1 (Polly)**

**Rationale**:
1. **Industry Standard**: Polly is the de facto standard for .NET resilience patterns
2. **Feature Completeness**: Provides all required features plus additional resilience patterns
3. **Maturity**: Battle-tested in production environments
4. **Community Support**: Large community, extensive documentation
5. **Future-Proof**: Active development, Microsoft integration
6. **Integration**: Seamless HttpClient integration with existing architecture
7. **Cost**: Free and open-source (Apache 2.0 license)

### Alternative Recommendation: **Option 2 (Microsoft.Extensions.Http.Resilience)**

**Use this option if**:
- Team prefers Microsoft-native solutions only
- Basic circuit breaker functionality is sufficient
- Want to minimize third-party dependencies
- Planning to use other Microsoft.Extensions packages

### Not Recommended: Options 3 & 4

**Reasons**:
- **Option 3 (Custom)**: High maintenance cost, potential for bugs, no community support
- **Option 4 (Steeltoe)**: Overkill for this use case, less popular, more complex

## Detailed Implementation Plan (Polly Approach)

### Phase 1: Package Installation and Configuration

#### 1.1 Package Installation
**Files to Modify**: `CoreLibraryCleanAdditionalService/Core.Library.Clean.AdditionalService.csproj`

```xml
<ItemGroup>
  <PackageReference Include="Polly" Version="8.4.0" />
  <PackageReference Include="Polly.Extensions.Http" Version="8.4.0" />
</ItemGroup>
```

#### 1.2 Configuration Structure
**Files to Modify**: `appsettings.json`, `appsettings.Development.json`

```json
{
  "CircuitBreaker": {
    "BookAPI": {
      "Enabled": true,
      "FailureThreshold": 5,
      "SamplingDuration": "00:01:00",
      "BreakDuration": "00:00:30",
      "HalfOpenAttempts": 3,
      "ExceptionsAllowedBeforeBreaking": 5
    },
    "PersonAPI": {
      "Enabled": true,
      "FailureThreshold": 5,
      "SamplingDuration": "00:01:00",
      "BreakDuration": "00:00:30",
      "HalfOpenAttempts": 3,
      "ExceptionsAllowedBeforeBreaking": 5
    },
    "Retry": {
      "MaxRetries": 3,
      "RetryDelay": "00:00:02",
      "ExponentialBackoff": true
    },
    "Timeout": {
      "DefaultTimeout": "00:00:30"
    }
  }
}
```

### Phase 2: Circuit Breaker Policy Creation

#### 2.1 Circuit Breaker Policy Factory
**New File**: `CoreLibraryCleanAdditionalService/Resilience/CircuitBreakerPolicyFactory.cs`

```csharp
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace Core.Library.Clean.AdditionalService.Resilience
{
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

        public IAsyncPolicy<HttpResponseMessage> CreateCircuitBreakerPolicy(string serviceName)
        {
            var config = _configuration.GetSection($"CircuitBreaker:{serviceName}");
            
            if (!config.GetValue<bool>("Enabled", true))
            {
                return Policy.NoOpAsync<HttpResponseMessage>();
            }

            var exceptionsAllowedBeforeBreaking = config.GetValue<int>("ExceptionsAllowedBeforeBreaking", 5);
            var durationOfBreak = config.GetValue<TimeSpan>("BreakDuration", TimeSpan.FromSeconds(30));

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

        public IAsyncPolicy<HttpResponseMessage> CreateRetryPolicy(string serviceName)
        {
            var config = _configuration.GetSection("CircuitBreaker:Retry");
            var maxRetries = config.GetValue<int>("MaxRetries", 3);
            var retryDelay = config.GetValue<TimeSpan>("RetryDelay", TimeSpan.FromSeconds(2));
            var exponentialBackoff = config.GetValue<bool>("ExponentialBackoff", true);

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

        public IAsyncPolicy<HttpResponseMessage> CreateTimeoutPolicy(string serviceName)
        {
            var config = _configuration.GetSection("CircuitBreaker:Timeout");
            var timeout = config.GetValue<TimeSpan>("DefaultTimeout", TimeSpan.FromSeconds(30));

            return Policy.TimeoutAsync<HttpResponseMessage>(timeout);
        }
    }
}
```

#### 2.2 Circuit Breaker Exception
**New File**: `CoreLibraryCleanAdditionalService/Resilience/CircuitBreakerException.cs`

```csharp
namespace Core.Library.Clean.AdditionalService.Resilience
{
    public class CircuitBreakerOpenException : Exception
    {
        public string ServiceName { get; }
        public DateTime BreakTime { get; }

        public CircuitBreakerOpenException(string serviceName, DateTime breakTime)
            : base($"Circuit breaker is open for service: {serviceName}")
        {
            ServiceName = serviceName;
            BreakTime = breakTime;
        }
    }
}
```

### Phase 3: Dependency Injection Configuration

#### 3.1 Update Dependency Injection
**File to Modify**: `CoreMVCClean/Code/DependencyInjection.cs`

```csharp
using Core.Library.Clean.AdditionalService;
using Core.Library.Clean.AdditionalService.Resilience;
using Polly.Extensions.Http;

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

            var bookAPIUrl = configuration["bookAPIUrl"];
            var personAPIUrl = configuration["personAPIUrl"];

            // Book API with circuit breaker
            services.AddHttpClient<BookDirector>(client =>
            {
                client.BaseAddress = new Uri(bookAPIUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddPolicyHandler((provider, message) => 
                provider.GetRequiredService<CircuitBreakerPolicyFactory>()
                    .CreateCircuitBreakerPolicy("BookAPI"))
            .AddPolicyHandler((provider, message) => 
                provider.GetRequiredService<CircuitBreakerPolicyFactory>()
                    .CreateRetryPolicy("BookAPI"))
            .AddPolicyHandler((provider, message) => 
                provider.GetRequiredService<CircuitBreakerPolicyFactory>()
                    .CreateTimeoutPolicy("BookAPI"));

            // Person API with circuit breaker
            services.AddHttpClient<PersonDirector>(client =>
            {
                client.BaseAddress = new Uri(personAPIUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddPolicyHandler((provider, message) => 
                provider.GetRequiredService<CircuitBreakerPolicyFactory>()
                    .CreateCircuitBreakerPolicy("PersonAPI"))
            .AddPolicyHandler((provider, message) => 
                provider.GetRequiredService<CircuitBreakerPolicyFactory>()
                    .CreateRetryPolicy("PersonAPI"))
            .AddPolicyHandler((provider, message) => 
                provider.GetRequiredService<CircuitBreakerPolicyFactory>()
                    .CreateTimeoutPolicy("PersonAPI"));

            return services;
        }
    }
}
```

### Phase 4: Director Layer Updates

#### 4.1 Update BaseDirector for Circuit Breaker Integration
**File to Modify**: `CoreLibraryCleanAdditionalService/Director/BaseDirector.cs`

```csharp
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Text;
using Core.Library.Clean.AdditionalService.Resilience;
using Polly;

namespace Core.Library.Clean.AdditionalService
{
    abstract public class BaseDirector
    {
        protected readonly HttpClient httpClient;
        protected readonly IHttpContextAccessor httpContextAccessor;
        protected readonly ILogger<BaseDirector> logger;

        protected BaseDirector(
            HttpClient _httpClient, 
            IHttpContextAccessor _httpContextAccessor,
            ILogger<BaseDirector> logger)
        {
            httpClient = _httpClient;
            httpContextAccessor = _httpContextAccessor;
            this.logger = logger;
        }

        protected void AddCorrelationIdHeader()
        {
            var correlationId = httpContextAccessor?.HttpContext?.Items["CorrelationId"]?.ToString();
            if (!string.IsNullOrEmpty(correlationId))
            {
                httpClient.DefaultRequestHeaders.Remove("X-Correlation-ID");
                httpClient.DefaultRequestHeaders.Add("X-Correlation-ID", correlationId);
            }
        }

        protected static StringContent CreateJsonContent(object content)
        {
            var json = JsonConvert.SerializeObject(content);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        protected async Task<T> ExecuteWithCircuitBreakerAsync<T>(
            Func<Task<T>> action,
            string operationName,
            CancellationToken cancellationToken)
        {
            try
            {
                return await action();
            }
            catch (BrokenCircuitException ex)
            {
                logger.LogWarning(
                    "Circuit breaker is open for {OperationName}. Exception: {Exception}",
                    operationName, ex.Message);
                
                throw new CircuitBreakerOpenException(
                    operationName, 
                    DateTime.UtcNow);
            }
            catch (TimeoutRejectedException ex)
            {
                logger.LogWarning(
                    "Timeout occurred for {OperationName}. Exception: {Exception}",
                    operationName, ex.Message);
                
                throw new ApiException(
                    ErrorCodes.EXTERNAL_API_TIMEOUT,
                    "External API request timed out",
                    504);
            }
        }

        protected static async Task<T> HandleResponseAsync<T>(
            HttpResponseMessage response,
            CancellationToken cancellationToken)
        {
            // Existing response handling logic remains the same
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content
                    .ReadAsStringAsync(cancellationToken)
                    .ConfigureAwait(false);

                try
                {
                    var errorResponse = JsonConvert.DeserializeObject<ApiErrorResponse>(errorContent);
                    if (errorResponse != null)
                    {
                        throw new ApiException(
                            errorResponse.Error.Code,
                            errorResponse.Error.Message,
                            errorResponse.Error.StatusCode);
                    }
                }
                catch
                {
                }

                var errorCode = response.StatusCode switch
                {
                    System.Net.HttpStatusCode.NotFound => ErrorCodes.NOT_FOUND,
                    System.Net.HttpStatusCode.BadRequest => ErrorCodes.BAD_REQUEST,
                    System.Net.HttpStatusCode.Unauthorized => ErrorCodes.UNAUTHORIZED,
                    System.Net.HttpStatusCode.Forbidden => ErrorCodes.FORBIDDEN,
                    _ => ErrorCodes.INTERNAL_SERVER_ERROR
                };

                var (message, statusCode) = ErrorCodeMapper.GetErrorDetails(errorCode);

                throw new ApiException(errorCode, message, statusCode);
            }

            var result = await response.Content
                .ReadAsStringAsync(cancellationToken)
                .ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(result))
            {
                return default;
            }

            try
            {
                var wrappedResponse = JsonConvert.DeserializeObject<ApiResponse<T>>(result);
                if (wrappedResponse != null && wrappedResponse.Success)
                {
                    return wrappedResponse.Data;
                }
            }
            catch
            {
            }

            return JsonConvert.DeserializeObject<T>(result);
        }
    }
}
```

#### 4.2 Update BookDirector Constructor
**File to Modify**: `CoreLibraryCleanAdditionalService/Director/BookDirector.cs`

```csharp
using Microsoft.AspNetCore.Http;

namespace Core.Library.Clean.AdditionalService
{
    public class BookDirector : BaseDirector, IEntityDirector<BookDTO, BookCreateDTO>
    {
        public BookDirector(
            HttpClient _httpClient, 
            IHttpContextAccessor _httpContextAccessor,
            ILogger<BookDirector> logger) 
            : base(_httpClient, _httpContextAccessor, logger)
        {
            Console.WriteLine(httpClient.BaseAddress);
        }

        // Update methods to use ExecuteWithCircuitBreakerAsync if needed
        // For now, Polly policies are applied at HttpClient level, so no changes needed
    }
}
```

#### 4.3 Update PersonDirector Constructor
**File to Modify**: `CoreLibraryCleanAdditionalService/Director/PersonDirector.cs`

```csharp
using Microsoft.AspNetCore.Http;

namespace Core.Library.Clean.AdditionalService
{
    public class PersonDirector : BaseDirector, IEntityDirector<PersonDTO, PersonCreateDTO>
    {
        public PersonDirector(
            HttpClient _httpClient, 
            IHttpContextAccessor _httpContextAccessor,
            ILogger<PersonDirector> logger) 
            : base(_httpClient, _httpContextAccessor, logger)
        {
            Console.WriteLine(httpClient.BaseAddress);
        }
    }
}
```

### Phase 5: Controller Layer Updates

#### 5.1 Update Controllers for Circuit Breaker Exceptions
**Files to Modify**: `CoreMVCClean/Controllers/BookController.cs`, `CoreMVCClean/Controllers/PersonController.cs`

```csharp
using Core.Library.Clean.AdditionalService;
using Core.Library.Clean.AdditionalService.Resilience;
using Microsoft.AspNetCore.Mvc;
using Core.MVC.Clean;

namespace Core.MVC.Clean.Controllers
{
    public class BookController : Controller
    {
        private readonly BookDirector apiClient;

        public BookController(BookDirector apiClient)
        {
            this.apiClient = apiClient;
        }

        public async Task<IActionResult> Index(string? search)
        {
            try
            {
                IEnumerable<BookDTO> result = null;

                if (!string.IsNullOrWhiteSpace(search))
                {
                   search = $"?search={Uri.EscapeDataString(search)}";
                   result = await apiClient.SearchEntitiesAsync(search, default).ConfigureAwait(true);
                   return View(result);
                }

                result = await apiClient.GetEntitiesAsync(default).ConfigureAwait(false);
                return View(result);
            }
            catch (CircuitBreakerOpenException ex)
            {
                var correlationId = HttpContext.GetCorrelationId();
                var errorViewModel = new ErrorViewModel
                {
                    RequestId = correlationId,
                    Message = "Book API is currently unavailable due to circuit breaker activation. Please try again later.",
                    ErrorCode = ErrorCodes.EXTERNAL_API_UNAVAILABLE,
                    StatusCode = 503
                };
                return View("CircuitBreakerOpen", errorViewModel);
            }
            catch (ApiException ex)
            {
                var correlationId = HttpContext.GetCorrelationId();
                var errorViewModel = new ErrorViewModel
                {
                    RequestId = correlationId,
                    Message = ex.Message,
                    ErrorCode = ex.ErrorCode,
                    StatusCode = ex.StatusCode
                };
                return View("Error", errorViewModel);
            }
        }

        // Similar updates for other methods...
    }
}
```

#### 5.2 Create Circuit Breaker Open View
**New File**: `CoreMVCClean/Views/Shared/CircuitBreakerOpen.cshtml`

```html
@model ErrorViewModel

@{
    ViewData["Title"] = "Service Unavailable";
}

<div class="alert alert-warning" role="alert">
    <h4 class="alert-heading">⚠️ Service Temporarily Unavailable</h4>
    <p>@Model.Message</p>
    <hr>
    @if (!string.IsNullOrEmpty(Model.RequestId))
    {
        <p class="mb-0">Request ID: @Model.RequestId</p>
    }
</div>

<div class="text-center mt-4">
    <p>The service is experiencing issues and the circuit breaker has been activated to protect the system.</p>
    <p>Please try again in a few moments.</p>
    <a href="@Url.Action("Index", "Home")" class="btn btn-primary">Return to Home</a>
</div>
```

### Phase 6: Monitoring and Health Checks

#### 6.1 Circuit Breaker Health Check Endpoint
**New File**: `CoreMVCClean/Controllers/HealthController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using Polly.CircuitBreaker;

namespace Core.MVC.Clean.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly CircuitBreakerPolicyFactory _circuitBreakerFactory;

        public HealthController(CircuitBreakerPolicyFactory circuitBreakerFactory)
        {
            _circuitBreakerFactory = circuitBreakerFactory;
        }

        [HttpGet("circuit-breaker-status")]
        public IActionResult GetCircuitBreakerStatus()
        {
            // This would need to be enhanced to actually track circuit states
            // For now, return a basic status
            return Ok(new
            {
                BookAPI = "Unknown",
                PersonAPI = "Unknown",
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
```

#### 6.2 Enhanced Logging
**Add to existing logging configuration**:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.AspNetCore": "Warning",
        "Polly": "Information"
      }
    }
  }
}
```

## Alternative Implementation Plan (Microsoft.Extensions.Http.Resilience)

### Key Differences from Polly Approach

#### Package Installation
```xml
<ItemGroup>
  <PackageReference Include="Microsoft.Extensions.Http.Resilience" Version="9.0.0" />
</ItemGroup>
```

#### Dependency Injection Configuration
```csharp
services.AddHttpClient<BookDirector>(client =>
{
    client.BaseAddress = new Uri(bookAPIUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddResilienceHandler("BookAPI", builder =>
{
    builder
        .AddCircuitBreaker(new CircuitBreakerStrategyOptions
        {
            FailureRatio = 0.1,
            SamplingDuration = TimeSpan.FromSeconds(30),
            MinimumThroughput = 3,
            BreakDuration = TimeSpan.FromSeconds(30),
            OnOpened = args =>
            {
                logger.LogWarning("Circuit opened for BookAPI");
            },
            OnClosed = args =>
            {
                logger.LogInformation("Circuit closed for BookAPI");
            }
        })
        .AddRetry(new RetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            Delay = TimeSpan.FromSeconds(2),
            BackoffType = DelayBackoffType.Exponential
        })
        .AddTimeout(TimeSpan.FromSeconds(30));
});
```

#### Advantages of This Approach
- Simpler configuration
- Microsoft-native implementation
- Less boilerplate code
- Built-in configuration support

#### Disadvantages
- Less flexible than Polly
- Fewer advanced features
- Smaller community ecosystem

## Configuration Options

### Circuit Breaker Settings

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `Enabled` | boolean | true | Enable/disable circuit breaker for service |
| `ExceptionsAllowedBeforeBreaking` | int | 5 | Number of failures before circuit opens |
| `BreakDuration` | TimeSpan | 00:00:30 | How long circuit stays open |
| `SamplingDuration` | TimeSpan | 00:01:00 | Time window for failure counting |
| `HalfOpenAttempts` | int | 3 | Allowed attempts in half-open state |

### Retry Settings

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `MaxRetries` | int | 3 | Maximum number of retry attempts |
| `RetryDelay` | TimeSpan | 00:00:02 | Delay between retry attempts |
| `ExponentialBackoff` | boolean | true | Use exponential backoff strategy |

### Timeout Settings

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `DefaultTimeout` | TimeSpan | 00:00:30 | Default timeout for HTTP requests |

## Testing Strategy

### Unit Tests

1. **Circuit Breaker Policy Tests**
   - Test circuit state transitions
   - Test failure counting
   - Test automatic recovery
   - Test configuration loading

2. **Director Integration Tests**
   - Test circuit breaker exception handling
   - Test retry logic
   - Test timeout handling
   - Test fallback behavior

3. **Controller Tests**
   - Test circuit breaker open exception handling
   - Test error view rendering
   - Test user experience during circuit open

### Integration Tests

1. **End-to-End Circuit Breaker Tests**
   - Test circuit breaker activation
   - Test circuit breaker recovery
   - Test monitoring endpoints
   - Test logging integration

2. **Failure Simulation Tests**
   - Simulate API failures
   - Simulate network timeouts
   - Simulate service unavailability
   - Test graceful degradation

### Manual Testing

1. **Circuit Breaker Activation**
   - Cause intentional API failures
   - Verify circuit opens after threshold
   - Verify circuit breaker open view
   - Verify logging of state changes

2. **Circuit Breaker Recovery**
   - Wait for break duration
   - Verify circuit transitions to half-open
   - Verify successful recovery
   - Verify circuit closes

## Monitoring and Observability

### Metrics to Track

1. **Circuit Breaker Metrics**
   - Circuit state (Closed/Open/Half-Open)
   - Failure count and rate
   - Success count and rate
   - Circuit break events
   - Recovery events

2. **API Call Metrics**
   - Request count
   - Success/failure rate
   - Average response time
   - Timeout rate
   - Retry count

3. **System Metrics**
   - CPU usage
   - Memory usage
   - Thread pool usage
   - HTTP connection pool usage

### Logging Requirements

1. **Circuit State Changes**
   - Log when circuit opens
   - Log when circuit closes
   - Log when circuit enters half-open
   - Include correlation IDs

2. **Failure Events**
   - Log each failure
   - Include error details
   - Include circuit state
   - Include retry information

3. **Performance Events**
   - Log slow requests
   - Log timeout events
   - Log retry attempts
   - Include timing information

### Health Check Endpoints

1. **Circuit Breaker Status**
   - `/api/health/circuit-breaker-status`
   - Return circuit states for all services
   - Include failure counts and rates
   - Include last state change times

2. **Service Health**
   - `/api/health/service-health`
   - Return overall service health
   - Include dependency health
   - Include circuit breaker impact

## Rollback Plan

### Graceful Rollback

1. **Configuration Rollback**
   - Set `CircuitBreaker:Enabled: false` in configuration
   - Circuit breaker becomes inactive
   - Normal HttpClient behavior resumes
   - No code changes required

2. **Code Rollback**
   - Remove Polly policy handlers from HttpClient registration
   - Restore original HttpClient configuration
   - Remove circuit breaker exception handling
   - Restore original error handling

3. **Partial Rollback**
   - Disable circuit breaker for specific services
   - Adjust circuit breaker thresholds
   - Modify retry policies
   - Change timeout values

## Security Considerations

### Security Requirements

1. **Configuration Security**
   - Secure circuit breaker configuration
   - Prevent unauthorized configuration changes
   - Validate configuration values
   - Audit configuration changes

2. **Logging Security**
   - No sensitive data in circuit breaker logs
   - Sanitize error messages
   - Protect correlation IDs
   - Secure log storage

3. **API Security**
   - Maintain authentication/authorization
   - Preserve security headers
   - Protect against abuse
   - Rate limiting consideration

## Performance Impact

### Expected Performance Impact

1. **Overhead Analysis**
   - Circuit breaker state checking: <1ms
   - Retry logic overhead: <5ms per retry
   - Timeout handling: <1ms
   - Policy execution: <2ms

2. **Resource Usage**
   - Memory: Minimal (~1KB per circuit breaker)
   - CPU: Negligible
   - Network: No additional overhead
   - Thread pool: Minimal impact

3. **Benefits**
   - Faster failure detection
   - Reduced resource waste
   - Improved system stability
   - Better user experience

## Migration Strategy

### Phase 1: Preparation (1-2 days)
- Install required packages
- Create configuration structure
- Set up logging and monitoring
- Prepare rollback procedures

### Phase 2: Implementation (2-3 days)
- Implement circuit breaker policies
- Update dependency injection
- Update director layer
- Update controller layer

### Phase 3: Testing (2-3 days)
- Unit testing
- Integration testing
- Manual testing
- Performance testing

### Phase 4: Deployment (1-2 days)
- Staging deployment
- Monitoring setup
- Production deployment
- Post-deployment validation

### Phase 5: Optimization (1-2 days)
- Tune circuit breaker parameters
- Optimize retry strategies
- Refine monitoring
- Update documentation

**Total Timeline**: 7-12 days

## Success Criteria

### Functional Requirements
- [ ] Circuit breaker activates after failure threshold
- [ ] Circuit breaker recovers automatically
- [ ] Retry logic works correctly
- [ ] Timeout handling prevents hanging requests
- [ ] Fallback behavior provides graceful degradation
- [ ] Configuration changes work without restart

### Non-Functional Requirements
- [ ] Performance overhead <5%
- [ ] Circuit breaker state transitions <100ms
- [ ] Memory usage <10MB
- [ ] 99.9% uptime maintained
- [ ] No cascading failures
- [ ] System stability improved

### Quality Requirements
- [ ] Code follows project conventions
- [ ] Unit tests achieve 80% coverage
- [ ] Integration tests validate scenarios
- [ ] Documentation is complete
- [ ] Build succeeds without warnings
- [ ] Security requirements met

## Risks and Mitigations

### Risk 1: Circuit Breaker Too Sensitive
**Mitigation**: 
- Start with conservative thresholds
- Monitor circuit breaker events
- Adjust thresholds based on metrics
- Provide configuration overrides

### Risk 2: Performance Degradation
**Mitigation**:
- Profile performance impact
- Optimize policy execution
- Monitor resource usage
- Set appropriate timeouts

### Risk 3: Configuration Errors
**Mitigation**:
- Validate configuration on startup
- Provide default values
- Log configuration issues
- Support hot reloading

### Risk 4: Integration Issues
**Mitigation**:
- Thorough testing in staging
- Gradual rollout
- Monitor integration points
- Quick rollback capability

## Future Enhancements

### Potential Improvements

1. **Advanced Circuit Breaker Features**
   - Failure rate based circuit breaking
   - Adaptive thresholds
   - Machine learning predictions
   - Predictive circuit breaking

2. **Enhanced Resilience Patterns**
   - Bulkhead isolation
   - Rate limiting
   - Cache aside pattern
   - Fallback chains

3. **Monitoring and Observability**
   - Real-time dashboards
   - Alerting integration
   - Performance monitoring
   - Distributed tracing

4. **Configuration Management**
   - Dynamic configuration updates
   - A/B testing support
   - Canary deployments
   - Feature flags integration

## References

### Related Documentation
- `api-standardization-specification.md` - API response model standardization
- `AGENTS.md` - Project agent configuration
- `project-spec.md` - Overall project specification

### External Resources
- [Polly Documentation](https://github.com/App-vNext/Polly)
- [Microsoft.Extensions.Http.Resilience](https://learn.microsoft.com/en-us/dotnet/core/resilience/http-resilience)
- [Circuit Breaker Pattern](https://docs.microsoft.com/en-us/azure/architecture/patterns/circuit-breaker)
- [Resilience Patterns](https://docs.microsoft.com/en-us/azure/architecture/patterns/category/resiliency)

---

**Document Version**: 1.0  
**Last Updated**: 2026-09-28  
**Status**: Requirements Analysis Complete  
**Next Steps**: Implementation Planning  
**Recommended Implementation**: Option 1 (Polly)