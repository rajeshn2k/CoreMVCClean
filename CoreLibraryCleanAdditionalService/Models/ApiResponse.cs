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