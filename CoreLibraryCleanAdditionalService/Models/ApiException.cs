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