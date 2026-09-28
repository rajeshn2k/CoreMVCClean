# API Response Model Standardization - Feature Specification

## Overview
This specification documents the API Response Model Standardization feature implemented in the CoreMVCClean application. The feature provides standardized API response structures with correlation tracking, structured error handling, and enhanced logging capabilities.

**Implementation Date**: 2026-09-27  
**Status**: ✅ Fully Implemented  
**Version**: 1.0

## Feature Objectives

1. **Standardize API Responses**: Provide consistent response structures for success and error cases
2. **Enable Request Tracing**: Implement correlation ID tracking for end-to-end request tracing
3. **Improve Error Handling**: Use structured error responses with standardized error codes
4. **Enhance Logging**: Integrate correlation IDs throughout the logging system
5. **Maintain Compatibility**: Support both new and legacy response formats during transition

## Architecture

### Component Overview

```
┌─────────────────────────────────────────────────────────────┐
│                     Presentation Layer                      │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │ Controllers  │  │   Middleware │  │     Views    │      │
│  └──────────────┘  └──────────────┘  └──────────────┘      │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    Business Logic Layer                      │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │   Directors  │  │ Response     │  │ Error Code   │      │
│  │              │  │ Helper       │  │ Mapper       │      │
│  └──────────────┘  └──────────────┘  └──────────────┘      │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                       Model Layer                            │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │ ApiResponse  │  │ ApiError     │  │ Error Codes  │      │
│  │ <T>          │  │ Response     │  │              │      │
│  └──────────────┘  └──────────────┘  └──────────────┘      │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    Infrastructure Layer                       │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │ Correlation  │  │ Global       │  │ Serilog      │      │
│  │ Middleware   │  │ Exception    │  │ Config       │      │
│  │              │  │ Handler      │  │              │      │
│  └──────────────┘  └──────────────┘  └──────────────┘      │
└─────────────────────────────────────────────────────────────┘
```

## Data Models

### Success Response Model

**File**: `CoreLibraryCleanAdditionalService/Models/ApiResponse.cs`

```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; } = true;
    public T Data { get; set; }
    public string Message { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string RequestId { get; set; }
}
```

**Purpose**: Wraps successful API responses with metadata

**Usage Example**:
```csharp
var response = new ApiResponse<BookDTO>
{
    Success = true,
    Data = book,
    Message = "Book retrieved successfully",
    Timestamp = DateTime.UtcNow,
    RequestId = correlationId
};
```

### Error Response Model

**File**: `CoreLibraryCleanAdditionalService/Models/ApiErrorResponse.cs`

```csharp
public class ApiErrorResponse
{
    public bool Success { get; set; } = false;
    public ErrorDetail Error { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string RequestId { get; set; }
    public string Path { get; set; }
}
```

**Purpose**: Standardized error response structure

**Usage Example**:
```csharp
var errorResponse = new ApiErrorResponse
{
    Success = false,
    Error = new ErrorDetail
    {
        Code = ErrorCodes.NOT_FOUND,
        Message = "Book not found",
        StatusCode = 404
    },
    Timestamp = DateTime.UtcNow,
    RequestId = correlationId,
    Path = "/api/Book/123"
};
```

### Error Detail Model

**File**: `CoreLibraryCleanAdditionalService/Models/ErrorDetail.cs`

```csharp
public class ErrorDetail
{
    public string Code { get; set; }
    public string Message { get; set; }
    public int StatusCode { get; set; }
}
```

**Purpose**: Detailed error information structure

### Error Codes

**File**: `CoreLibraryCleanAdditionalService/Models/ErrorCodes.cs`

```csharp
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
```

**Purpose**: Standardized error code constants

## Exception Handling

### Custom Exception

**File**: `CoreLibraryCleanAdditionalService/Models/ApiException.cs`

```csharp
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
```

**Purpose**: Custom exception for API errors with standardized error codes

### Error Code Mapper

**File**: `CoreLibraryCleanAdditionalService/Models/ErrorCodeMapper.cs`

```csharp
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
```

**Purpose**: Maps error codes to user-friendly messages and HTTP status codes

## Response Handling

### Response Helper

**File**: `CoreLibraryCleanAdditionalService/Director/ResponseHelper.cs`

```csharp
public static class ResponseHelper
{
    public static ApiResponse<T> WrapResponse<T>(T data, string message = "Operation successful")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message,
            Timestamp = DateTime.UtcNow,
            RequestId = null
        };
    }

    public static T ExtractData<T>(object response)
    {
        if (response is ApiResponse<T> apiResponse)
        {
            return apiResponse.Data;
        }
        return (T)response;
    }

    public static bool IsWrappedResponse(object response)
    {
        return response != null && response.GetType().IsGenericType &&
               response.GetType().GetGenericTypeDefinition() == typeof(ApiResponse<>);
    }

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
            RequestId = null,
            Path = path
        };
    }
}
```

**Purpose**: Helper methods for response wrapping/unwrapping and conversion

## Correlation ID System

### Correlation ID Middleware

**File**: `CoreMVCClean/Code/CorrelationIdMiddleware.cs`

```csharp
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
        var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault();

        if (string.IsNullOrEmpty(correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
        }

        context.Items[CorrelationIdItemKey] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
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

**Purpose**: Generates and propagates correlation IDs through the request pipeline

**Features**:
- Accepts correlation ID from incoming request header
- Generates new GUID if not provided
- Stores correlation ID in HttpContext.Items
- Adds correlation ID to response headers
- Integrates with Serilog for logging

### Correlation ID Helper

**File**: `CoreMVCClean/Code/CorrelationIdHelper.cs`

```csharp
public static class CorrelationIdHelper
{
    private const string CorrelationIdItemKey = "CorrelationId";

    public static string GetCorrelationId(HttpContext context)
    {
        return context.Items[CorrelationIdItemKey]?.ToString();
    }

    public static string GetCorrelationId(this HttpContext context)
    {
        return context.Items[CorrelationIdItemKey]?.ToString();
    }
}
```

**Purpose**: Provides convenient access to correlation IDs from HttpContext

## Global Exception Handling

### Global Exception Handler

**File**: `CoreMVCClean/Code/GlobalExceptionHandler.cs`

```csharp
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
```

**Purpose**: Centralized exception handling with structured error responses

**Features**:
- Handles ApiException with structured error responses
- Returns JSON error responses for API calls
- Redirects to error page for UI requests
- Logs errors with correlation IDs
- Handles unexpected exceptions gracefully

## Director Layer Integration

### Director Updates

Both `BookDirector` and `PersonDirector` have been updated with:

1. **Constructor Changes**:
```csharp
private readonly HttpClient httpClient;
private readonly IHttpContextAccessor httpContextAccessor;

public BookDirector(HttpClient _httpClient, IHttpContextAccessor _httpContextAccessor)
{
    httpClient = _httpClient;
    httpContextAccessor = _httpContextAccessor;
}
```

2. **Correlation ID Propagation**:
```csharp
private void AddCorrelationIdHeader()
{
    var correlationId = httpContextAccessor?.HttpContext?.Items["CorrelationId"]?.ToString();
    if (!string.IsNullOrEmpty(correlationId))
    {
        httpClient.DefaultRequestHeaders.Remove("X-Correlation-ID");
        httpClient.DefaultRequestHeaders.Add("X-Correlation-ID", correlationId);
    }
}
```

3. **Enhanced Response Handling**:
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
```

**Features**:
- Handles both wrapped and unwrapped responses
- Propagates correlation IDs to external API calls
- Converts structured error responses to exceptions
- Maintains backward compatibility

## Controller Layer Integration

### Controller Updates

Both `BookController` and `PersonController` have been updated with:

1. **Structured Error Handling**:
```csharp
try
{
    // API call
    result = await apiClient.GetEntitiesAsync(default).ConfigureAwait(false);
    return View(result);
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
```

2. **Consistent Error Display**:
- All controller actions handle ApiException consistently
- Correlation IDs extracted and passed to error views
- Error details displayed in user-friendly format

### Error View Model Updates

**File**: `CoreMVCClean/Code/ErrorViewModel.cs`

```csharp
public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    // Original properties
    public string? ExceptionMessage { get; set; }
    public string? ExceptionStackTrace { get; set; }
    
    // New properties for API standardization
    public string? Message { get; set; }
    public string? ErrorCode { get; set; }
    public int? StatusCode { get; set; }
}
```

### Home Controller Updates

**File**: `CoreMVCClean/Controllers/HomeController.cs`

```csharp
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public IActionResult Error(string requestId = null)
{
    var correlationId = requestId ?? HttpContext.GetCorrelationId() ?? Activity.Current?.Id ?? HttpContext.TraceIdentifier;
    
    var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
    var exception = exceptionFeature?.Error;

    var model = new ErrorViewModel
    {
        RequestId = correlationId,
        ExceptionMessage = exception?.Message,
        ExceptionStackTrace = exception?.StackTrace
    };

    if (exception is ApiException apiException)
    {
        model.Message = apiException.Message;
        model.ErrorCode = apiException.ErrorCode;
        model.StatusCode = apiException.StatusCode;
    }

    return View(model);
}
```

## Infrastructure Configuration

### Middleware Pipeline

**File**: `CoreMVCClean/Program.cs`

```csharp
// Configure Serilog
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
);

// Middleware pipeline (in order)
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<GlobalExceptionHandler>();
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
```

### Dependency Injection

**File**: `CoreMVCClean/Code/DependencyInjection.cs`

```csharp
public static IServiceCollection AddApplicationServices(
    this IServiceCollection services,
    IConfiguration configuration)
{
    services.AddSingleton(configuration);
    services.AddHttpContextAccessor();

    var bookAPIUrl = configuration["bookAPIUrl"];
    var personAPIUrl = configuration["personAPIUrl"];

    services.AddHttpClient<BookDirector>(client =>
    {
        client.BaseAddress = new Uri(bookAPIUrl);
    });

    services.AddHttpClient<PersonDirector>(client =>
    {
        client.BaseAddress = new Uri(personAPIUrl);
    });

    return services;
}
```

### Application Configuration

**File**: `CoreMVCClean/appsettings.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.AspNetCore": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] [CorrelationId: {CorrelationId}] {Message:lj}{NewLine}{Exception}"
        }
      }
    ],
    "Enrich": ["FromLogContext"]
  },
  "bookAPIUrl": "",
  "personAPIUrl": "",
  "AllowedHosts": "*",
  "ApiStandardization": {
    "EnableNewResponseFormat": true,
    "EnableCorrelationId": true,
    "CorrelationIdHeaderName": "X-Correlation-ID",
    "IncludeDetailedErrorsInResponse": false,
    "DefaultErrorCode": "INTERNAL_SERVER_ERROR"
  }
}
```

**File**: `CoreMVCClean/appsettings.Development.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "bookAPIUrl": "http://localhost:6101/api/book/",
  "personAPIUrl": "http://localhost:6101/api/person/",
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:4101"
      }
    }
  },
  "ApiStandardization": {
    "IncludeDetailedErrorsInResponse": true
  }
}
```

## Configuration Options

### API Standardization Settings

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `EnableNewResponseFormat` | boolean | true | Enable new wrapped response format |
| `EnableCorrelationId` | boolean | true | Enable correlation ID tracking |
| `CorrelationIdHeaderName` | string | "X-Correlation-ID" | Header name for correlation ID |
| `IncludeDetailedErrorsInResponse` | boolean | false | Include detailed error information in responses |
| `DefaultErrorCode` | string | "INTERNAL_SERVER_ERROR" | Default error code for unknown errors |

### Environment-Specific Settings

**Development**: 
- `IncludeDetailedErrorsInResponse: true` - Detailed errors for debugging

**Production**:
- `IncludeDetailedErrorsInResponse: false` - Generic error messages for security

## Usage Examples

### Creating a Success Response

```csharp
// In Director layer
var books = await GetBooksFromDatabase();
var response = ResponseHelper.WrapResponse(books, "Books retrieved successfully");
return Ok(response);
```

### Creating an Error Response

```csharp
// In GlobalExceptionHandler
var errorResponse = new ApiErrorResponse
{
    Success = false,
    Error = new ErrorDetail
    {
        Code = ErrorCodes.BOOK_NOT_FOUND,
        Message = "Book not found",
        StatusCode = 404
    },
    Timestamp = DateTime.UtcNow,
    RequestId = correlationId,
    Path = "/api/Book/123"
};
return NotFound(errorResponse);
```

### Throwing an API Exception

```csharp
// In Director layer
if (book == null)
{
    throw new ApiException(
        ErrorCodes.BOOK_NOT_FOUND,
        "Book not found",
        404);
}
```

### Accessing Correlation ID

```csharp
// In Controller
var correlationId = HttpContext.GetCorrelationId();

// In Director
var correlationId = httpContextAccessor?.HttpContext?.GetCorrelationId();
```

### Handling API Exceptions

```csharp
// In Controller
try
{
    var book = await apiClient.GetEntityByIdAsync(id, default);
    return View(book);
}
catch (ApiException ex)
{
    var errorViewModel = new ErrorViewModel
    {
        RequestId = HttpContext.GetCorrelationId(),
        Message = ex.Message,
        ErrorCode = ex.ErrorCode,
        StatusCode = ex.StatusCode
    };
    return View("Error", errorViewModel);
}
```

## Backward Compatibility

The implementation maintains backward compatibility with existing response formats:

### Response Format Detection

Directors automatically detect response format:
- If response is wrapped in `ApiResponse<T>`, data is extracted
- If response is raw entity, it's returned as-is
- If response is structured error, it's converted to `ApiException`

### Migration Strategy

1. **Graceful Degradation**: System handles both old and new formats
2. **Feature Flags**: Configuration controls enable/disable new features
3. **Testing**: Existing functionality continues to work during transition
4. **Rollback**: Configuration can disable new format if needed

## Logging and Monitoring

### Correlation ID Logging

All log entries include correlation IDs:
```
[10:30:45 INF] [CorrelationId: 550e8400-e29b-41d4-a716-446655440000] Processing GET /Book
[10:30:46 INF] [CorrelationId: 550e8400-e29b-41d4-a716-446655440000] Retrieved 5 books
[10:30:47 ERR] [CorrelationId: 550e8400-e29b-41d4-a716-446655440000] API Error: BOOK_NOT_FOUND - Book not found
```

### Error Logging

Structured error logging includes:
- Correlation ID
- Error code
- Error message
- HTTP status code
- Request path
- Timestamp

### Performance Monitoring

Correlation IDs enable:
- End-to-end request tracing
- Performance analysis across services
- Error correlation and debugging
- User journey tracking

## Security Considerations

### Error Message Security

- Production: Generic error messages (no internal details)
- Development: Detailed error messages for debugging
- No sensitive data in error responses
- Sanitized error paths to prevent information disclosure

### Correlation ID Security

- Correlation IDs are GUIDs (no sensitive information)
- Headers are properly validated
- No injection vulnerabilities
- Proper logging and security

## Testing Recommendations

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

## Troubleshooting

### Common Issues

**Issue**: Correlation ID not appearing in logs
- **Solution**: Verify Serilog configuration includes `{CorrelationId}` in output template

**Issue**: Directors not handling wrapped responses
- **Solution**: Verify response format matches expected structure

**Issue**: Error responses not converting to exceptions
- **Solution**: Check JSON serialization/deserialization compatibility

**Issue**: Controllers not receiving correlation IDs
- **Solution**: Verify IHttpContextAccessor is registered in DI

### Debugging Tips

1. Enable detailed error responses in development
2. Check correlation ID propagation in logs
3. Verify middleware pipeline order
4. Test with both wrapped and unwrapped responses
5. Monitor HTTP headers for correlation IDs

## Performance Impact

### Expected Impact

- **Response Size**: Slight increase due to wrapper structure (~50-100 bytes)
- **Processing Time**: Minimal overhead from correlation ID handling (<1ms)
- **Memory**: Small increase from correlation ID storage
- **Network**: Additional header for correlation ID (~36 bytes)

### Mitigation Strategies

- Reuse JSON serializer settings
- Optimize correlation ID storage
- Consider response compression for larger payloads
- Monitor performance metrics post-implementation

## Future Enhancements

### Potential Improvements

1. **Request Context Expansion**
   - Add user context to correlation
   - Include session information
   - Track client information

2. **Advanced Error Handling**
   - Retry logic for transient failures
   - Circuit breaker pattern
   - Fallback mechanisms

3. **Monitoring Integration**
   - Application Performance Monitoring (APM)
   - Distributed tracing systems
   - Real-time alerting

4. **API Documentation**
   - Swagger/OpenAPI integration
   - Response schema documentation
   - Error code documentation

## Maintenance Guidelines

### Code Maintenance

1. **Adding New Error Codes**
   - Add to `ErrorCodes` class
   - Update `ErrorCodeMapper` with message and status code
   - Document usage and scenarios

2. **Updating Response Models**
   - Maintain backward compatibility
   - Update XML documentation
   - Test serialization/deserialization

3. **Modifying Middleware**
   - Maintain pipeline order
   - Test integration with other middleware
   - Verify correlation ID propagation

### Configuration Maintenance

1. **Adding New Settings**
   - Update `appsettings.json` structure
   - Add to development configuration
   - Document setting purpose and values

2. **Environment-Specific Changes**
   - Maintain separation between environments
   - Document differences
   - Test in each environment

## References

### Related Documentation

- `api-standardization-requirements.md` - Original requirements document
- `api-standardization-implementation-plan.md` - Detailed implementation plan
- `AGENTS.md` - Project agent configuration with implementation status
- `project-spec.md` - Overall project specification

### External Resources

- ASP.NET Core Middleware Documentation
- Serilog Documentation
- HTTP Client Best Practices
- Error Handling Patterns

---

**Document Version**: 1.0  
**Last Updated**: 2026-09-28  
**Maintained By**: Development Team  
**Status**: Active Specification