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