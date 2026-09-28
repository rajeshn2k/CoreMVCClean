using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;
using Core.Library.Clean.AdditionalService.Resilience;
using Polly.CircuitBreaker;
using Polly.Timeout;

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
            Console.WriteLine(httpClient.BaseAddress);
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

        protected static StringContent CreateJsonContent(object content)
        {
            var json = JsonConvert.SerializeObject(content);

            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        protected static async Task<T> HandleResponseAsync<T>(
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
    }
}
