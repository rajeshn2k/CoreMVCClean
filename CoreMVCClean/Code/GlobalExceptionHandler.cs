using Core.Library.Clean.AdditionalService;

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