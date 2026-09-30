using Core.Library.Clean.AdditionalService;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Net.Http.Headers;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace Core.MVC.Clean;

public sealed class GlobalExceptionHandler
{
    private const string ErrorPath = "/Home/Error";
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
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.GetCorrelationId();

        LogException(exception, correlationId);

        /*
         * If the response has already started, we cannot safely replace it.
         */
        if (context.Response.HasStarted)
        {
            _logger.LogWarning("[CorrelationId: {CorrelationId}] Response has already started. Exception cannot be handled by GlobalExceptionHandler.", correlationId);

            throw exception;
        }

        var statusCode = GetStatusCode(exception);

        /*
         * Preserve the original request information BEFORE changing
         * Request.Path.
         */
        var originalPath = context.Request.Path;

        /*
         * JSON/API/AJAX request
         */
        if (IsJsonRequest(context))
        {
            context.Response.Clear();
            context.Response.StatusCode = statusCode;

            await WriteJsonErrorResponseAsync(context, exception, correlationId, statusCode);

            return;
        }

        /*
         * MVC HTML request
         *
         * We want to execute /Home/Error using the SAME HttpContext.
         *
         * Merely changing Request.Path is not enough because ASP.NET Core
         * may still have the original BookController.Index endpoint selected.
         */

        var exceptionFeature = new ExceptionHandlerFeature
        {
            Error = exception,
            Path = originalPath
        };

        context.Features.Set<IExceptionHandlerFeature>(exceptionFeature);
        context.Features.Set<IExceptionHandlerPathFeature>(exceptionFeature);

        context.Response.Clear();

        /*
         * Keep the HTTP status code.
         */
        context.Response.StatusCode = statusCode;

        /*
         * Change the path to the error action.
         */
        context.Request.Path = ErrorPath;

        /*
         * IMPORTANT:
         *
         * Clear the route values from the original endpoint.
         */
        context.Request.RouteValues.Clear();

        /*
         * IMPORTANT:
         *
         * Tell ASP.NET Core that the current endpoint is no longer valid.
         *
         * This allows UseRouting() to execute routing again and select
         * HomeController.Error instead of BookController.Index.
         */
        context.SetEndpoint(null);

        /*
         * Execute the remaining middleware pipeline again.
         */
        await _next(context);
    }

    private void LogException(
        Exception exception,
        string correlationId)
    {
        switch (exception)
        {
            case BrokenCircuitException:
                _logger.LogWarning(exception, "[CorrelationId: {CorrelationId}] Polly circuit breaker is open.", correlationId);
                break;

            case TimeoutRejectedException:
                _logger.LogWarning(exception, "[CorrelationId: {CorrelationId}] Polly timeout occurred.", correlationId);
                break;

            case ApiException apiException:
                _logger.LogError(exception, "[CorrelationId: {CorrelationId}] API Error. Code={ErrorCode}, StatusCode={StatusCode}, Message={Message}", correlationId, apiException.ErrorCode, apiException.StatusCode, apiException.Message);
                break;

            default:
                _logger.LogError(exception, "[CorrelationId: {CorrelationId}] Unhandled exception.", correlationId);
                break;
        }
    }

    private static int GetStatusCode(Exception exception)
    {
        return exception switch
        {
            ApiException apiException
                when apiException.StatusCode >= 400 &&
                     apiException.StatusCode <= 599
                => apiException.StatusCode,

            BrokenCircuitException
                => StatusCodes.Status503ServiceUnavailable,

            TimeoutRejectedException
                => StatusCodes.Status503ServiceUnavailable,

            HttpRequestException
                => StatusCodes.Status502BadGateway,

            TaskCanceledException
                => StatusCodes.Status504GatewayTimeout,

            _ => StatusCodes.Status500InternalServerError
        };
    }

    private static async Task WriteJsonErrorResponseAsync(
        HttpContext context,
        Exception exception,
        string correlationId,
        int statusCode)
    {
        var errorCode = exception switch
        {
            ApiException apiException => apiException.ErrorCode,
            BrokenCircuitException => "POLLY_CIRCUIT_OPEN",
            TimeoutRejectedException => "POLLY_TIMEOUT",
            HttpRequestException => "HTTP_REQUEST_ERROR",
            TaskCanceledException => "REQUEST_TIMEOUT",
            _ => "UNHANDLED_EXCEPTION"
        };

        var errorResponse = new ApiErrorResponse
        {
            Success = false,

            Error = new ErrorDetail
            {
                Code = errorCode,

                /*
                 * You requested complete exception information.
                 */
                Message = exception.ToString(),

                StatusCode = statusCode
            },

            Timestamp = DateTime.UtcNow,
            RequestId = correlationId,

            /*
             * At this point this is still the original request path.
             */
            Path = context.Request.Path
        };

        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(errorResponse);
    }

    private static bool IsJsonRequest(HttpContext context)
    {
        var request = context.Request;

        /*
         * API route
         */
        if (request.Path.StartsWithSegments(
                "/api",
                StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        /*
         * AJAX
         */
        if (request.Headers.TryGetValue("X-Requested-With", out var requestedWith) &&
            string.Equals(requestedWith.ToString(), "XMLHttpRequest", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        /*
         * Request Content-Type
         */
        if (!string.IsNullOrWhiteSpace(request.ContentType) && request.ContentType.Contains("application/json", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        /*
         * Accept header
         */
        var accept = request.Headers[HeaderNames.Accept].ToString();

        if (accept.Contains("application/json", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }
}