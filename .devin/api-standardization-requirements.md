# API Response Model Standardization Requirements

## Overview
This specification captures the requirements for adapting the CoreMVCClean application to support standardized API response models. The application needs to transition from direct entity responses to wrapped response structures that include success/error metadata, correlation tracking, and standardized error handling.

**Important Note**: These requirements assume that external API integration will be completed first. The current in-memory implementation will be replaced with HTTP-based external API calls before implementing these standardization requirements.

## Current State Analysis

### Existing Response Patterns
The current implementation uses direct entity returns from API calls:

**Current BookDirector Pattern:**
```csharp
// Success case - returns entity directly
public async Task<BookDTO> GetEntityByIdAsync(string entityId, CancellationToken cancellationToken)
{
    var response = await httpClient.GetAsync(requestUrl, cancellationToken);
    return await HandleResponseAsync<BookDTO>(response, cancellationToken);
}

// Error handling - throws exceptions
private static async Task<T> HandleResponseAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
{
    if (!response.IsSuccessStatusCode)
    {
        var error = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new HttpRequestException($"Request failed with status code {(int)response.StatusCode}...");
    }
    return JsonConvert.DeserializeObject<T>(result);
}
```

**Current Controller Pattern:**
```csharp
// Controllers work directly with entities
public async Task<ActionResult> Edit(string bookId)
{
    BookDTO book = await apiClient.GetEntityByIdAsync(bookId, default);
    return View(book);
}
```

### Limitations of Current Approach
- No standardized response wrapper
- Inconsistent error handling across operations
- No correlation tracking for request tracing
- Limited error context for debugging
- No standardized error codes
- No timestamp tracking
- Controllers need to handle raw exceptions

## New Contract Requirements

### Success Response Model
**New Structure:**
```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }           // Always true for success responses
    public T Data { get; set; }                 // The actual entity/data
    public string Message { get; set; }          // Success message
    public DateTime Timestamp { get; set; }      // UTC timestamp of response
    public string RequestId { get; set; }        // Correlation ID for tracing
}
```

**Success Response Example:**
```csharp
var response = new ApiResponse<BookDTO>
{
    Success = true,
    Data = book,
    Message = "Book retrieved successfully",
    Timestamp = DateTime.UtcNow,
    RequestId = HttpContext.Items["CorrelationId"]?.ToString()
};
return Ok(response);
```

### Error Response Model
**New Structure:**
```csharp
public class ApiErrorResponse
{
    public bool Success { get; set; }           // Always false for error responses
    public ErrorDetail Error { get; set; }      // Detailed error information
    public DateTime Timestamp { get; set; }      // UTC timestamp of error
    public string RequestId { get; set; }        // Correlation ID for tracing
    public string Path { get; set; }             // Request path that caused error
}

public class ErrorDetail
{
    public string Code { get; set; }             // Standardized error code
    public string Message { get; set; }          // User-friendly error message
    public int StatusCode { get; set; }         // HTTP status code
}
```

**Error Response Example:**
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
    RequestId = HttpContext.Items["CorrelationId"]?.ToString(),
    Path = $"/api/Book/{id}"
};
return NotFound(errorResponse);
```

## Standardized Error Codes

### Error Code Enumeration
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

## Correlation ID Requirements

### Correlation ID Implementation
**Middleware Requirements:**
- Add correlation ID middleware to generate/request correlation IDs
- Store correlation ID in `HttpContext.Items["CorrelationId"]`
- Propagate correlation ID to external API calls via headers
- Log correlation ID in all error and information messages

**Header Standards:**
- Incoming request header: `X-Correlation-ID` (optional)
- Outgoing request header: `X-Correlation-ID` (required)
- Fallback: Generate new GUID if not provided

**Implementation Requirements:**
```csharp
// Middleware to handle correlation IDs
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    
    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault();
        
        if (string.IsNullOrEmpty(correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
        }
        
        context.Items["CorrelationId"] = correlationId;
        context.Response.OnStarting(() =>
        {
            context.Response.Headers["X-Correlation-ID"] = correlationId;
            return Task.CompletedTask;
        });
        
        await _next(context);
    }
}
```

## Impact Analysis

### Components Requiring Changes

#### 1. Models Layer (CoreLibraryCleanAdditionalService/Models/)
**New Models Required:**
- `ApiResponse<T>` - Generic success response wrapper
- `ApiErrorResponse` - Error response wrapper
- `ErrorDetail` - Error detail structure
- `ErrorCodes` - Static error code constants

**Impact:**
- Add new model classes to Models folder
- No changes to existing entity models (Book, Person, DTOs)

#### 2. Director Layer (CoreLibraryCleanAdditionalService/Director/)
**Required Changes:**
- Update `BookDirector.HandleResponseAsync<T>()` to handle new response wrapper
- Update `PersonDirector.HandleResponseAsync<T>()` to handle new response wrapper
- Add correlation ID propagation to HTTP client requests
- Update error handling to use new error response structure
- Add methods to extract data from wrapped responses

**Impact:**
- Modify existing response handling logic
- Add correlation ID header to HTTP requests
- Update exception handling to use structured error details
- Maintain backward compatibility during transition

#### 3. Controller Layer (CoreMVCClean/Controllers/)
**Required Changes:**
- Update controllers to handle wrapped responses from Directors
- Add correlation ID extraction from HttpContext
- Update error handling to use structured error responses
- Update views to work with wrapped data structure
- Add error display logic in views

**Impact:**
- Modify all controller actions that call Director methods
- Update view models to handle wrapped responses
- Add error handling UI components
- Update search and CRUD operations

#### 4. Infrastructure Layer (CoreMVCClean/Code/)
**Required Changes:**
- Add correlation ID middleware registration in `Program.cs`
- Update `DependencyInjection.cs` to configure HTTP client with correlation ID headers
- Add error handling middleware for structured error responses

**Impact:**
- Modify application startup configuration
- Add new middleware components
- Update HTTP client configuration

#### 5. Configuration (appsettings.json)
**Required Changes:**
- Add correlation ID configuration if needed
- Add error code mappings if external
- Configure logging to include correlation IDs

**Impact:**
- Add new configuration sections
- Update logging configuration

### Data Flow Changes

#### Current Data Flow:
```
Controller → Director → External API → Director → Controller → View
                     (Raw Entity)         (Raw Entity)
```

#### New Data Flow:
```
Controller → Director → External API → Director → Controller → View
                     (ApiResponse<T>)   (Extract Data)    (Raw Entity)
                     (ApiErrorResponse) (Handle Error)    (Error Display)
```

## Implementation Strategy

### Incremental Approach
The API standardization will be implemented incrementally in 8 phases to ensure:
- Each phase can be tested independently
- Changes can be deployed incrementally
- Rollback is easier if issues arise
- Testing can be focused on specific components
- Reduced risk of breaking existing functionality

### Phase Dependencies
- **Phase 1** (Models) → Foundation for all subsequent phases
- **Phase 2** (Error Codes) → Independent, can be done in parallel with Phase 1
- **Phase 3** (Director Success) → Depends on Phase 1
- **Phase 4** (Director Error) → Depends on Phases 1, 2, 3
- **Phase 5** (Correlation Infrastructure) → Independent, can be done in parallel with earlier phases
- **Phase 6** (Director Correlation) → Depends on Phase 5
- **Phase 7** (Controller Updates) → Depends on Phases 3, 4, 6
- **Phase 8** (Error Integration) → Depends on all previous phases

### Testing Strategy
Each phase includes:
- Unit tests for new components
- Integration tests for modified components
- Backward compatibility tests
- Manual testing of affected functionality

## Implementation Requirements

### Phase 1: Model Layer Updates (Foundation)
1. Create `ApiResponse<T>` model class
2. Create `ApiErrorResponse` model class
3. Create `ErrorDetail` model class
4. Create `ErrorCodes` static class
5. Add XML documentation for all new models
6. Add unit tests for model serialization/deserialization

### Phase 2: Error Code Standardization (Basic Error Handling)
1. Implement `ErrorCodes` static class with all error constants
2. Update existing exception handling to use standardized error codes
3. Add error code to exception messages
4. Create error code mapping for user-friendly messages
5. Test error code generation and mapping

### Phase 3: Director Layer Response Wrapping (Success Responses)
1. Update `BookDirector.HandleResponseAsync<T>()` to:
   - Wrap successful responses in `ApiResponse<T>`
   - Add basic timestamp and message to responses
   - Maintain backward compatibility for unwrapped responses

2. Update `PersonDirector.HandleResponseAsync<T>()` with same changes

3. Add helper methods:
   - `WrapResponse<T>()` - Wrap successful responses
   - `ExtractDataFromResponse<T>()` - Extract data from wrapped responses
   - Support both wrapped and unwrapped response formats

4. Test Director methods with both response formats

### Phase 4: Director Layer Error Handling (Error Responses)
1. Update Directors to handle `ApiErrorResponse` from external APIs
2. Convert `ApiErrorResponse` to structured exceptions
3. Add error detail extraction from error responses
4. Update error messages with structured details
5. Add error response to exception conversion helper
6. Test error handling with structured error responses

### Phase 5: Correlation ID Infrastructure (Request Tracing)
1. Create `CorrelationIdMiddleware` class
2. Register middleware in `Program.cs` pipeline
3. Implement correlation ID generation and propagation
4. Update HTTP client configuration in `DependencyInjection.cs`
5. Add correlation ID to HTTP client default headers
6. Configure logging to include correlation IDs
7. Test correlation ID propagation through request chain

### Phase 6: Director Layer Correlation Integration
1. Update Directors to extract correlation ID from response headers
2. Add correlation ID to wrapped responses
3. Propagate correlation ID through HTTP client calls
4. Add correlation ID to error responses
5. Test correlation ID in Director layer

### Phase 7: Controller Updates (Response Handling)
1. Update `BookController` actions:
   - Handle wrapped responses from Directors
   - Extract data for views
   - Handle structured errors
   - Pass correlation ID to views for logging

2. Update `PersonController` with same changes

3. Update views to:
   - Handle wrapped data structure
   - Display error messages from structured responses
   - Show correlation IDs for debugging
   - Maintain current UI functionality

4. Test controller actions with new response format

### Phase 8: Error Handling Integration (Complete Error System)
1. Update global exception handler to work with structured errors
2. Add error logging with correlation IDs
3. Update error views to display structured error information
4. Complete error code mappings for user-friendly messages
5. Test complete error handling flow

## Backward Compatibility Requirements

### Transition Strategy
1. **Graceful Degradation**: Directors should handle both old and new response formats during transition
2. **Incremental Implementation**: Implement response models first, then error codes, then correlation IDs in phases
3. **Testing**: Maintain existing functionality while implementing new format
4. **Rollback**: Plan to revert changes if external API doesn't support new format yet

### Compatibility Considerations
- External API may not support new format immediately
- Directors should detect response format and handle accordingly
- Maintain existing exception types for compatibility
- Views should work with both formats during transition
- Each phase should be independently testable and deployable

## Testing Requirements

### Unit Tests Required
1. Test `ApiResponse<T>` model serialization/deserialization
2. Test `ApiErrorResponse` model serialization/deserialization
3. Test Director response unwrapping logic
4. Test error response to exception conversion
5. Test correlation ID propagation
6. Test backward compatibility handling

### Integration Tests Required
1. Test end-to-end success flows with new response format
2. Test error flows with structured error responses
3. Test correlation ID propagation through request chain
4. Test controller handling of wrapped responses
5. Test view rendering with new data structure

### Manual Testing Required
1. Test Book CRUD operations with new format
2. Test Person CRUD operations with new format
3. Test search functionality
4. Test error scenarios (404, 500, etc.)
5. Verify correlation IDs in logs and responses

## Performance Considerations

### Performance Impact
- Additional JSON deserialization layer for response wrapping
- Minimal overhead from correlation ID handling
- Slightly increased response size due to wrapper structure

### Mitigation Strategies
- Reuse JSON serializer settings
- Optimize correlation ID storage (string vs GUID)
- Consider response compression for larger payloads
- Monitor performance metrics after implementation

## Security Considerations

### Security Requirements
- Correlation IDs should not contain sensitive information
- Error messages should not expose internal system details
- Validate correlation ID format to prevent injection attacks
- Ensure correlation IDs are properly logged and secured

### Error Message Security
- Use generic error messages for external errors
- Include detailed errors only in logs with correlation IDs
- Sanitize error paths to prevent information disclosure
- Implement proper error code mappings

## Logging Requirements

### Enhanced Logging
1. Include correlation ID in all log entries
2. Log structured error details from API responses
3. Log request/response timestamps
4. Log error codes and status codes
5. Log request paths for error tracking

### Log Format
```
[CorrelationId: {guid}] [Timestamp: {utc}] [Level: {level}] 
[ErrorCode: {code}] [Message: {message}] [Path: {path}]
```

## Configuration Requirements

### New Configuration Settings
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

### Environment-Specific Settings
- Development: Enable detailed errors for debugging
- Production: Disable detailed errors, use generic messages
- Testing: Enable correlation ID tracking

## Migration Strategy

### Step-by-Step Migration
1. **Phase 1**: Implement new models and infrastructure
2. **Phase 2**: Update Directors with dual-format support
3. **Phase 3**: Update Controllers with dual-format support  
4. **Phase 4**: Test thoroughly with both formats
5. **Phase 5**: Enable new format in configuration
6. **Phase 6**: Monitor and validate
7. **Phase 7**: Remove old format support

### Rollback Plan
- Configuration flag to disable new format
- Maintain old response handling code until stable
- Monitor error rates and performance metrics
- Quick rollback capability if issues arise

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

## Open Questions

1. **External API Timeline**: When will the external API support the new response format?
2. **Backward Compatibility Duration**: How long should we maintain dual-format support?
3. **Error Code Granularity**: How detailed should the error codes be?
4. **Correlation ID Format**: Should we use GUIDs or a different format?
5. **Monitoring**: What metrics should we track post-implementation?

## Dependencies

### External Dependencies
- External API must support new response format
- External API must handle correlation ID headers
- External API must return standardized error codes

### Internal Dependencies
- Existing entity models (no changes required)
- Existing Director interface (maintain compatibility)
- Existing Controller actions (update implementation only)
- **Note**: This work is separate from SQLite database migration - these concerns are intentionally separated

## Risks and Mitigations

### Risk 1: External API Not Ready
**Mitigation**: Implement dual-format support with feature flags

### Risk 2: Performance Degradation
**Mitigation**: Profile and optimize JSON serialization, monitor metrics

### Risk 3: Breaking Changes
**Mitigation**: Maintain backward compatibility, thorough testing

### Risk 4: Complex Error Handling
**Mitigation**: Centralize error handling logic, clear documentation

### Risk 5: Correlation ID Propagation Issues
**Mitigation**: Comprehensive testing, fallback mechanisms

## Timeline Estimate

- **Phase 1 (Model Layer)**: 1-2 days
- **Phase 2 (Error Codes)**: 1 day
- **Phase 3 (Director Success Responses)**: 2-3 days
- **Phase 4 (Director Error Handling)**: 2-3 days
- **Phase 5 (Correlation Infrastructure)**: 1-2 days
- **Phase 6 (Director Correlation Integration)**: 1-2 days
- **Phase 7 (Controller Updates)**: 2-3 days
- **Phase 8 (Error Handling Integration)**: 1-2 days
- **Testing & Documentation**: 2-3 days
- **Total**: 13-21 days

## Conclusion

This standardization will significantly improve the application's error handling, debugging capabilities, and API contract consistency. The changes are substantial but can be implemented incrementally with proper backward compatibility support. The structured approach with correlation tracking will enhance operational support and troubleshooting capabilities.

---

**Document Status**: Requirements Complete (Updated for Incremental Implementation)
**Implementation Status**: ✅ Fully Implemented (2026-09-27)
**Last Updated**: 2026-09-27
**Implementation Approach**: Incremental - 8 phases with independent testing and deployment
**See Also**: `api-standardization-implementation-plan.md` for detailed implementation plan