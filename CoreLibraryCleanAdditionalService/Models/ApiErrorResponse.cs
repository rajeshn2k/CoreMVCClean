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