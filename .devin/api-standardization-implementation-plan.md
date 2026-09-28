# API Response Model Standardization - Implementation Plan

## Overview
This document provides a detailed implementation plan for the API Response Model Standardization requirements. The plan follows the 8-phase incremental approach defined in the requirements document.

**Current State**: External API integration is assumed to be completed first. Directors use HttpClient to communicate with external APIs and return raw entities.

**Target State**: All API responses are wrapped in standardized structures with correlation tracking, error codes, and structured error handling.

## Phase 1: Model Layer Updates (Foundation)

### Objective
Create the foundational model classes for standardized API responses.

### Tasks

#### 1.1 Create ApiResponse<T> Model
**File**: `CoreLibraryCleanAdditionalService/Models/ApiResponse.cs`
```csharp
namespace Core.Library.Clean.AdditionalService
{
    /// <summary>
    /// Standardized API response wrapper for successful operations
    /// </summary>
    /// <typeparam name="T">Type of data being returned</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// Always true for success responses
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// The actual entity/data returned by the API
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// Success message describing the operation
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// UTC timestamp of when the response was generated
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Correlation ID for request tracing
        /// </summary>
        public string RequestId { get; set; }
    }
}
```

#### 1.2 Create ApiErrorResponse Model
**File**: `CoreLibraryCleanAdditionalService/Models/ApiErrorResponse.cs`
```csharp
namespace Core.Library.Clean.AdditionalService
{
    /// <summary>
    /// Standardized API error response wrapper
    /// </summary>
    public class ApiErrorResponse
    {
        /// <summary>
        /// Always false for error responses
        /// </summary>
        public bool Success { get; set; } = false;

        /// <summary>
        /// Detailed error information
        /// </summary>
        public ErrorDetail Error { get; set; }

        /// <summary>
        /// UTC timestamp of when the error occurred
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Correlation ID for request tracing
        /// </summary>
        public string RequestId { get; set; }

        /// <summary>
        /// Request path that caused the error
        /// </summary>
        public string Path { get; set; }
    }
}
```

#### 1.3 Create ErrorDetail Model
**File**: `CoreLibraryCleanAdditionalService/Models/ErrorDetail.cs`
```csharp
namespace Core.Library.Clean.AdditionalService
{
    /// <summary>
    /// Detailed error information structure
    /// </summary>
    public class ErrorDetail
    {
        /// <summary>
        /// Standardized error code from ErrorCodes class
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// User-friendly error message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// HTTP status code
        /// </summary>
        public int StatusCode { get; set; }
    }
}
```

#### 1.4 Create ErrorCodes Static Class
**File**: `CoreLibraryCleanAdditionalService/Models/ErrorCodes.cs`
```csharp
namespace Core.Library.Clean.AdditionalService
{
    /// <summary>
    /// Standardized error codes for API responses
    /// </summary>
    public static class ErrorCodes
    {
        // Common errors
        public const string NOT_FOUND = "NOT_FOUND";
        public const string VALIDATION_ERROR = "VALIDATION_ERROR";
        public const string UNAUTHORIZED = "UNAUTHORIZED";
        public const string FORBIDDEN = "FORBIDDEN";
        public const string INTERNAL_SERVER_ERROR = "INTERNAL_SERVER_ERROR";
        public const string BAD_REQUEST = "BAD_REQUEST";
        public const string CONFLICT = "CONFLICT";

        // Entity-specific errors
        public const string BOOK_NOT_FOUND = "BOOK_NOT_FOUND";
        public const string PERSON_NOT_FOUND = "PERSON_NOT_FOUND";
        public const string BOOK_CREATION_FAILED = "BOOK_CREATION_FAILED";
        public const string PERSON_CREATION_FAILED = "PERSON_CREATION_FAILED";
        public const string BOOK_UPDATE_FAILED = "BOOK_UPDATE_FAILED";
        public const string PERSON_UPDATE_FAILED = "PERSON_UPDATE_FAILED";
        public const string BOOK_DELETE_FAILED = "BOOK_DELETE_FAILED";
        public const string PERSON_DELETE_FAILED = "PERSON_DELETE_FAILED";

        // External API errors
        public const string EXTERNAL_API_ERROR = "EXTERNAL_API_ERROR";
        public const string EXTERNAL_API_TIMEOUT = "EXTERNAL_API_TIMEOUT";
        public const string EXTERNAL_API_UNAVAILABLE = "EXTERNAL_API_UNAVAILABLE";
    }
}
```

#### 1.5 Add Unit Tests for Models
**File**: Create test project (if not exists) and add tests for:
- `ApiResponse<T>` serialization/deserialization
- `ApiErrorResponse` serialization/deserialization
- `ErrorDetail` serialization/deserialization
- Default values and property initialization

### Validation Criteria
- [ ] All model classes compile without errors
- [ ] XML documentation is complete
- [ ] Unit tests pass for all models
- [ ] Models follow project naming conventions
- [ ] Models are in correct namespace

---

## Phase 2: Error Code Standardization (Basic Error Handling)

### Objective
Implement error code standardization and update existing exception handling.

### Tasks

#### 2.1 Create Custom Exception Classes
**File**: `CoreLibraryCleanAdditionalService/Models/ApiException.cs`
```csharp
namespace Core.Library.Clean.AdditionalService
{
    /// <summary>
    /// Custom exception for API errors with standardized error codes
    /// </summary>
    public class ApiException : Exception
    {
        public string ErrorCode { get; }
        public int StatusCode { get; }

        public ApiException(string errorCode, string message, int statusCode) 
            : base(message)
        {
            ErrorCode = errorCode;
            StatusCode = statusCode;
        }

        public ApiException(string errorCode, string message, int statusCode, Exception innerException) 
            : base(message, innerException)
        {
            ErrorCode = errorCode;
            StatusCode = statusCode;
        }
    }
}
```

#### 2.2 Create Error Code Mapper
**File**: `CoreLibraryCleanAdditionalService/Models/ErrorCodeMapper.cs`
```csharp
namespace Core.Library.Clean.AdditionalService
{
    /// <summary>
    /// Maps error codes to user-friendly messages and status codes
    /// </summary>
    public static class ErrorCodeMapper
    {
        public static (string Message, int StatusCode) GetErrorDetails(string errorCode)
        {
            return errorCode switch
            {
                ErrorCodes.NOT_FOUND => ("Resource not found", 404),
                ErrorCodes.BOOK_NOT_FOUND => ("Book not found", 404),
                ErrorCodes.PERSON_NOT_FOUND => ("Person not found", 404),
                ErrorCodes.VALIDATION_ERROR => ("Validation failed", 400),
                ErrorCodes.UNAUTHORIZED => ("Unauthorized access", 401),
                ErrorCodes.FORBIDDEN => ("Access forbidden", 403),
                ErrorCodes.BAD_REQUEST => ("Bad request", 400),
                ErrorCodes.CONFLICT => ("Resource conflict", 409),
                ErrorCodes.BOOK_CREATION_FAILED => ("Failed to create book", 500),
                ErrorCodes.PERSON_CREATION_FAILED => ("Failed to create person", 500),
                ErrorCodes.BOOK_UPDATE_FAILED => ("Failed to update book", 500),
                ErrorCodes.PERSON_UPDATE_FAILED => ("Failed to update person", 500),
                ErrorCodes.BOOK_DELETE_FAILED => ("Failed to delete book", 500),
                ErrorCodes.PERSON_DELETE_FAILED => ("Failed to delete person", 500),
                ErrorCodes.EXTERNAL_API_ERROR => ("External API error", 502),
                ErrorCodes.EXTERNAL_API_TIMEOUT => ("External API timeout", 504),
                ErrorCodes.EXTERNAL_API_UNAVAILABLE => ("External API unavailable", 503),
                _ => ("An error occurred", 500)
            };
        }
    }
}
```

#### 2.3 Update Director Exception Handling
**Files**: 
- `CoreLibraryCleanAdditionalService/Director/BookDirector.cs`
- `CoreLibraryCleanAdditionalService/Director/PersonDirector.cs`

Modify `HandleResponseAsync` method to use new exception types:
```csharp
private static async Task<T> HandleResponseAsync<T>(
    HttpResponseMessage response,
    CancellationToken cancellationToken)
{
    if (!response.IsSuccessStatusCode)
    {
        var error = await response.Content
            .ReadAsStringAsync(cancellationToken)
            .ConfigureAwait(false);

        // Map status code to error code
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

    // ... rest of existing code
}
```

### Validation Criteria
- [ ] Custom exception class created and tested
- [ ] Error code mapper handles all defined error codes
- [ ] Directors use new exception types
- [ ] Existing functionality still works
- [ ] Error messages are user-friendly

---

## Phase 3: Director Layer Response Wrapping (Success Responses)

### Objective
Update Directors to wrap successful responses in ApiResponse<T> while maintaining backward compatibility.

### Tasks

#### 3.1 Create Response Helper Methods
**File**: `CoreLibraryCleanAdditionalService/Director/ResponseHelper.cs`
```csharp
using Newtonsoft.Json;

namespace Core.Library.Clean.AdditionalService
{
    /// <summary>
    /// Helper methods for handling API responses
    /// </summary>
    public static class ResponseHelper
    {
        /// <summary>
        /// Wraps data in ApiResponse<T> structure
        /// </summary>
        public static ApiResponse<T> WrapResponse<T>(T data, string message = "Operation successful")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = data,
                Message = message,
                Timestamp = DateTime.UtcNow,
                RequestId = null // Will be set by correlation ID middleware
            };
        }

        /// <summary>
        /// Extracts data from ApiResponse<T> or returns raw data if not wrapped
        /// </summary>
        public static T ExtractData<T>(object response)
        {
            if (response is ApiResponse<T> apiResponse)
            {
                return apiResponse.Data;
            }

            // Backward compatibility: return raw data
            return (T)response;
        }

        /// <summary>
        /// Checks if response is wrapped in ApiResponse<T>
        /// </summary>
        public static bool IsWrappedResponse(object response)
        {
            return response != null && response.GetType().IsGenericType &&
                   response.GetType().GetGenericTypeDefinition() == typeof(ApiResponse<>);
        }
    }
}
```

#### 3.2 Update BookDirector Response Handling
**File**: `CoreLibraryCleanAdditionalService/Director/BookDirector.cs`

Modify `HandleResponseAsync` to detect and handle wrapped responses:
```csharp
private static async Task<T> HandleResponseAsync<T>(
    HttpResponseMessage response,
    CancellationToken cancellationToken)
{
    if (!response.IsSuccessStatusCode)
    {
        var error = await response.Content
            .ReadAsStringAsync(cancellationToken)
            .ConfigureAwait(false);

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

    // Try to deserialize as ApiResponse<T> first
    try
    {
        var wrappedResponse = JsonConvert.DeserializeObject<ApiResponse<T>>(result);
        if (wrappedResponse != null && wrappedResponse.Success)
        {
            return wrappedResponse.Data;
        }
    }
    catch (JsonException)
    {
        // Not a wrapped response, fall through to raw deserialization
    }

    // Backward compatibility: deserialize as raw type
    return JsonConvert.DeserializeObject<T>(result);
}
```

#### 3.3 Update PersonDirector Response Handling
**File**: `CoreLibraryCleanAdditionalService/Director/PersonDirector.cs`

Apply the same changes as BookDirector.

### Validation Criteria
- [ ] Response helper methods work correctly
- [ ] Directors handle both wrapped and unwrapped responses
- [ ] Backward compatibility maintained
- [ ] Unit tests for response handling pass
- [ ] Existing controller actions still work

---

## Phase 4: Director Layer Error Handling (Error Responses)

### Objective
Update Directors to handle ApiErrorResponse from external APIs and convert to structured exceptions.

### Tasks

#### 4.1 Update Director Error Handling
**Files**: 
- `CoreLibraryCleanAdditionalService/Director/BookDirector.cs`
- `CoreLibraryCleanAdditionalService/Director/PersonDirector.cs`

Enhance `HandleResponseAsync` to handle error responses:
```csharp
private static async Task<T> HandleResponseAsync<T>(
    HttpResponseMessage response,
    CancellationToken cancellationToken)
{
    if (!response.IsSuccessStatusCode)
    {
        var errorContent = await response.Content
            .ReadAsStringAsync(cancellationToken)
            .ConfigureAwait(false);

        // Try to deserialize as ApiErrorResponse
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
        catch (JsonException)
        {
            // Not a structured error response, fall through to default handling
        }

        // Default error handling
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

    // ... rest of existing success handling
}
```

#### 4.2 Add Error Response Conversion Helper
**File**: `CoreLibraryCleanAdditionalService/Director/ResponseHelper.cs`

Add method to convert exceptions to error responses:
```csharp
/// <summary>
/// Converts ApiException to ApiErrorResponse
/// </summary>
public static ApiErrorResponse ConvertToErrorResponse(ApiException exception, string path)
{
    return new ApiErrorResponse
    {
        Success = false,
        Error = new ErrorDetail
        {
            Code = exception.ErrorCode,
            Message = exception.Message,
            StatusCode = exception.StatusCode
        },
        Timestamp = DateTime.UtcNow,
        RequestId = null, // Will be set by correlation ID middleware
        Path = path
    };
}
```

### Validation Criteria
- [ ] Directors handle structured error responses
- [ ] Error responses converted to exceptions correctly
- [ ] Backward compatibility for non-structured errors
- [ ] Unit tests for error handling pass
- [ ] Error propagation works correctly

---

## Phase 5: Correlation ID Infrastructure (Request Tracing)

### Objective
Create correlation ID middleware and infrastructure for request tracing.

### Tasks

#### 5.1 Create CorrelationIdMiddleware
**File**: `CoreMVCClean/Code/CorrelationIdMiddleware.cs`
```csharp
namespace Core.MVC.Clean
{
    /// <summary>
    /// Middleware to handle correlation IDs for request tracing
    /// </summary>
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private const string CorrelationIdHeader = "X-Correlation-ID";
        private const string CorrelationIdItemKey = "CorrelationId";

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Extract correlation ID from request header or generate new one
            var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault();

            if (string.IsNullOrEmpty(correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
            }

            // Store in HttpContext for use in application
            context.Items[CorrelationIdItemKey] = correlationId;

            // Add correlation ID to response header
            context.Response.OnStarting(() =>
            {
                context.Response.Headers[CorrelationIdHeader] = correlationId;
                return Task.CompletedTask;
            });

            await _next(context);
        }
    }
}
```

#### 5.2 Register Middleware in Program.cs
**File**: `CoreMVCClean/Program.cs`

Add middleware registration at the top of the pipeline:
```csharp
// Add correlation ID middleware at the top of the pipeline
app.UseMiddleware<CorrelationIdMiddleware>();

// Existing exception handler
app.UseExceptionHandler("/Home/Error");
```

#### 5.3 Update HTTP Client Configuration
**File**: `CoreMVCClean/Code/DependencyInjection.cs`

Add correlation ID header configuration:
```csharp
services.AddHttpClient<BookDirector>(client =>
{
    client.BaseAddress = new Uri(bookAPIUrl);
    client.DefaultRequestHeaders.Add("X-Correlation-ID", string.Empty); // Placeholder
});

services.AddHttpClient<PersonDirector>(client =>
{
    client.BaseAddress = new Uri(personAPIUrl);
    client.DefaultRequestHeaders.Add("X-Correlation-ID", string.Empty); // Placeholder
});
```

#### 5.4 Create Correlation ID Helper
**File**: `CoreMVCClean/Code/CorrelationIdHelper.cs`
```csharp
using Microsoft.AspNetCore.Http;

namespace Core.MVC.Clean
{
    /// <summary>
    /// Helper for accessing correlation ID from HttpContext
    /// </summary>
    public static class CorrelationIdHelper
    {
        private const string CorrelationIdItemKey = "CorrelationId";

        /// <summary>
        /// Gets the correlation ID from current HttpContext
        /// </summary>
        public static string GetCorrelationId(HttpContext context)
        {
            return context.Items[CorrelationIdItemKey]?.ToString();
        }

        /// <summary>
        /// Gets the correlation ID from current HttpContext (extension method)
        /// </summary>
        public static string GetCorrelationId(this HttpContext context)
        {
            return context.Items[CorrelationIdItemKey]?.ToString();
        }
    }
}
```

### Validation Criteria
- [ ] Correlation ID middleware generates and propagates IDs
- [ ] Correlation IDs appear in response headers
- [ ] Middleware is registered in correct pipeline position
- [ ] HTTP client configuration includes correlation ID headers
- [ ] Helper methods work correctly

---

## Phase 6: Director Layer Correlation Integration

### Objective
Integrate correlation ID propagation in Director layer.

### Tasks

#### 6.1 Update Directors to Accept Correlation ID
**Files**: 
- `CoreLibraryCleanAdditionalService/Director/BookDirector.cs`
- `CoreLibraryCleanAdditionalService/Director/PersonDirector.cs`

Add correlation ID parameter to constructor:
```csharp
private readonly HttpClient httpClient;
private readonly IHttpContextAccessor httpContextAccessor;

public BookDirector(HttpClient _httpClient, IHttpContextAccessor _httpContextAccessor)
{
    httpClient = _httpClient;
    httpContextAccessor = _httpContextAccessor;
}
```

#### 6.2 Add Correlation ID to HTTP Requests
Modify each HTTP call to include correlation ID header:
```csharp
private void AddCorrelationIdHeader()
{
    var correlationId = httpContextAccessor?.HttpContext?.GetCorrelationId();
    if (!string.IsNullOrEmpty(correlationId))
    {
        httpClient.DefaultRequestHeaders.Remove("X-Correlation-ID");
        httpClient.DefaultRequestHeaders.Add("X-Correlation-ID", correlationId);
    }
}

// Add this call at the start of each method
public async Task<IEnumerable<BookDTO>> GetEntitiesAsync(CancellationToken cancellationToken)
{
    AddCorrelationIdHeader();
    var requestUrl = "";
    // ... rest of method
}
```

#### 6.3 Update Dependency Injection
**File**: `CoreMVCClean/Code/DependencyInjection.cs`

Add IHttpContextAccessor registration:
```csharp
services.AddHttpContextAccessor();

services.AddHttpClient<BookDirector>(client =>
{
    client.BaseAddress = new Uri(bookAPIUrl);
});
```

#### 6.4 Extract Correlation ID from Responses
Update response handling to extract correlation ID from response headers:
```csharp
private string ExtractCorrelationId(HttpResponseMessage response)
{
    if (response.Headers.TryGetValues("X-Correlation-ID", out var values))
    {
        return values.FirstOrDefault();
    }
    return httpContextAccessor?.HttpContext?.GetCorrelationId();
}
```

### Validation Criteria
- [ ] Correlation IDs propagate to external API calls
- [ ] Correlation IDs extracted from response headers
- [ ] IHttpContextAccessor properly registered
- [ ] Directors include correlation ID in all requests
- [ ] End-to-end correlation ID tracing works

---

## Phase 7: Controller Updates (Response Handling)

### Objective
Update Controllers to handle wrapped responses and extract data for views.

### Tasks

#### 7.1 Update BookController
**File**: `CoreMVCClean/Controllers/BookController.cs`

Update actions to handle wrapped responses:
```csharp
public async Task<IActionResult> Index(string? search)
{
    try
    {
        IEnumerable<BookDTO> result = null;

        if (!string.IsNullOrWhiteSpace(search))
        {
           search = $"?search={Uri.EscapeDataString(search)}";
           result = await apiClient.SearchEntitiesAsync(search, default).ConfigureAwait(true);
        }
        else
        {
            result = await apiClient.GetEntitiesAsync(default).ConfigureAwait(false);
        }

        // Directors now handle unwrapping, so result is already the data
        return View(result);
    }
    catch (ApiException ex)
    {
        // Handle structured errors
        ModelState.AddModelError(string.Empty, ex.Message);
        return View("Error", ex);
    }
}
```

#### 7.2 Update PersonController
**File**: `CoreMVCClean/Controllers/PersonController.cs`

Apply similar changes as BookController.

#### 7.3 Update Error Handling in Controllers
Add consistent error handling across all controller actions:
```csharp
private IActionResult HandleApiException(ApiException ex)
{
    // Log error with correlation ID
    var correlationId = HttpContext.GetCorrelationId();
    // Log: [CorrelationId: {correlationId}] Error: {ex.ErrorCode} - {ex.Message}

    // Return appropriate error view
    return View("Error", new ErrorViewModel
    {
        RequestId = correlationId,
        Message = ex.Message
    });
}
```

#### 7.4 Update Views for Error Display
**File**: `CoreMVCClean/Views/Shared/Error.cshtml`

Update to display structured error information:
```html
@model ErrorViewModel

<h1>Error</h1>
@if (!string.IsNullOrEmpty(Model.RequestId))
{
    <p>Request ID: @Model.RequestId</p>
}
@if (!string.IsNullOrEmpty(Model.Message))
{
    <p>@Model.Message</p>
}
```

### Validation Criteria
- [ ] Controllers handle wrapped responses correctly
- [ ] Error handling is consistent across controllers
- [ ] Views display error information properly
- [ ] Correlation IDs shown in error views
- [ ] Existing functionality still works

---

## Phase 8: Error Handling Integration (Complete Error System)

### Objective
Integrate complete error handling system with correlation IDs and structured responses.

### Tasks

#### 8.1 Update Global Exception Handler
**File**: `CoreMVCClean/Code/GlobalExceptionHandler.cs`

Create custom exception handler middleware:
```csharp
namespace Core.MVC.Clean
{
    /// <summary>
    /// Global exception handler for structured error responses
    /// </summary>
    public class GlobalExceptionHandler
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ApiException ex)
            {
                await HandleApiExceptionAsync(context, ex);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleApiExceptionAsync(HttpContext context, ApiException ex)
        {
            var correlationId = context.GetCorrelationId();
            
            _logger.LogError(ex, 
                "[CorrelationId: {CorrelationId}] API Error: {ErrorCode} - {Message}", 
                correlationId, ex.ErrorCode, ex.Message);

            context.Response.StatusCode = ex.StatusCode;
            
            if (context.Request.Headers["Accept"].Contains("application/json"))
            {
                var errorResponse = new ApiErrorResponse
                {
                    Success = false,
                    Error = new ErrorDetail
                    {
                        Code = ex.ErrorCode,
                        Message = ex.Message,
                        StatusCode = ex.StatusCode
                    },
                    Timestamp = DateTime.UtcNow,
                    RequestId = correlationId,
                    Path = context.Request.Path
                };

                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(errorResponse);
            }
            else
            {
                context.Response.Redirect($"/Home/Error?requestId={correlationId}");
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var correlationId = context.GetCorrelationId();
            
            _logger.LogError(ex, 
                "[CorrelationId: {CorrelationId}] Unhandled Exception: {Message}", 
                correlationId, ex.Message);

            context.Response.Redirect($"/Home/Error?requestId={correlationId}");
        }
    }
}
```

#### 8.2 Register Global Exception Handler
**File**: `CoreMVCClean/Program.cs`

Replace existing exception handler:
```csharp
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<GlobalExceptionHandler>();
// Remove: app.UseExceptionHandler("/Home/Error");
```

#### 8.3 Update Error View Model
**File**: `CoreMVCClean/Code/ErrorViewModel.cs`

Add correlation ID and error details:
```csharp
namespace Core.MVC.Clean
{
    public class ErrorViewModel
    {
        public string RequestId { get; set; }
        public string Message { get; set; }
        public string ErrorCode { get; set; }
        public int? StatusCode { get; set; }
    }
}
```

#### 8.4 Update Home Controller Error Action
**File**: `CoreMVCClean/Controllers/HomeController.cs`

Update error action to handle correlation ID:
```csharp
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public IActionResult Error(string requestId = null)
{
    var errorViewModel = new ErrorViewModel
    {
        RequestId = requestId ?? Activity.Current?.Id ?? HttpContext.TraceIdentifier
    };

    return View(errorViewModel);
}
```

#### 8.5 Configure Logging with Correlation IDs
**File**: `CoreMVCClean/Program.cs`

Update Serilog configuration to include correlation IDs:
```csharp
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: 
        "[{Timestamp:HH:mm:ss} {Level:u3}] [CorrelationId: {CorrelationId}] {Message:lj}{NewLine}{Exception}")
);
```

### Validation Criteria
- [ ] Global exception handler handles all exception types
- [ ] Correlation IDs included in all log entries
- [ ] Structured error responses returned for API calls
- [ ] Error views display correlation IDs and error details
- [ ] Logging configuration includes correlation IDs
- [ ] Complete error handling flow works end-to-end

---

## Testing Strategy

### Unit Tests
1. **Model Tests**
   - ApiResponse<T> serialization/deserialization
   - ApiErrorResponse serialization/deserialization
   - ErrorDetail property validation
   - ErrorCodes constant validation

2. **Director Tests**
   - Response wrapping/unwrapping
   - Error response handling
   - Correlation ID propagation
   - Backward compatibility

3. **Helper Tests**
   - ResponseHelper methods
   - ErrorCodeMapper mappings
   - CorrelationIdHelper methods

### Integration Tests
1. **End-to-end API flows**
   - Success flows with wrapped responses
   - Error flows with structured errors
   - Correlation ID propagation through request chain

2. **Controller Tests**
   - Controller actions with wrapped responses
   - Error handling in controllers
   - View rendering with error information

### Manual Testing
1. **Functional Testing**
   - Book CRUD operations
   - Person CRUD operations
   - Search functionality
   - Error scenarios (404, 500, etc.)

2. **Correlation ID Testing**
   - Verify correlation IDs in logs
   - Verify correlation IDs in response headers
   - Verify correlation ID propagation

---

## Configuration Updates

### appsettings.json
Add configuration section:
```json
{
  "ApiStandardization": {
    "EnableNewResponseFormat": true,
    "EnableCorrelationId": true,
    "CorrelationIdHeaderName": "X-Correlation-ID",
    "IncludeDetailedErrorsInResponse": false,
    "DefaultErrorCode": "INTERNAL_SERVER_ERROR"
  }
}
```

### appsettings.Development.json
```json
{
  "ApiStandardization": {
    "IncludeDetailedErrorsInResponse": true
  }
}
```

---

## Rollback Plan

### Phase-by-Phase Rollback
Each phase can be independently rolled back:
1. **Phase 1-2**: Remove new model classes, revert exception handling
2. **Phase 3-4**: Revert Director response handling changes
3. **Phase 5-6**: Remove correlation ID middleware and infrastructure
4. **Phase 7**: Revert Controller changes
5. **Phase 8**: Remove global exception handler, restore original error handling

### Configuration Rollback
Set `EnableNewResponseFormat` to false to disable new format while keeping code in place.

---

## Success Criteria

### Functional Requirements
- [ ] All CRUD operations work with new response format
- [ ] Error handling uses structured error responses
- [ ] Correlation IDs propagate through entire request chain
- [ ] Controllers properly unwrap responses
- [ ] Views display data correctly from wrapped responses
- [ ] Error views show structured error information

### Non-Functional Requirements
- [ ] Performance impact is minimal (<5% slowdown)
- [ ] Backward compatibility maintained during transition
- [ ] Security requirements met (no sensitive data exposure)
- [ ] Logging includes correlation IDs consistently
- [ ] Error codes are standardized and documented

### Quality Requirements
- [ ] Code follows project coding conventions
- [ ] Unit tests achieve adequate coverage
- [ ] Integration tests validate end-to-end flows
- [ ] Documentation is updated
- [ ] Build succeeds without warnings

---

## Timeline Summary

- **Phase 1 (Model Layer)**: 1-2 days
- **Phase 2 (Error Codes)**: 1 day
- **Phase 3 (Director Success)**: 2-3 days
- **Phase 4 (Director Error)**: 2-3 days
- **Phase 5 (Correlation Infrastructure)**: 1-2 days
- **Phase 6 (Director Correlation)**: 1-2 days
- **Phase 7 (Controller Updates)**: 2-3 days
- **Phase 8 (Error Integration)**: 1-2 days
- **Testing & Documentation**: 2-3 days
- **Total**: 13-21 days

---

**Document Status**: Implementation Plan Complete
**Implementation Status**: ✅ Fully Implemented (2026-09-27)
**Last Updated**: 2026-09-27
**All 8 Phases Completed Successfully**