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
                RequestId = null
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
                RequestId = null,
                Path = path
            };
        }
    }
}