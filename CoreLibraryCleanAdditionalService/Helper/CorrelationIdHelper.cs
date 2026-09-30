using Microsoft.AspNetCore.Http;

namespace Core.Library.Clean.AdditionalService
{
    /// <summary>
    /// Helper for accessing correlation ID from HttpContext
    /// </summary>
    public static class CorrelationIdHelper
    {
        private const string CorrelationIdItemKey = "CorrelationId";

        /// <summary>
        /// Gets the correlation ID from current IHttpContextAccessor context (extension method)
        /// Implementation for DIRECTOR LAYER
        /// </summary>
        public static string GetCorrelationId(this IHttpContextAccessor context)
        {
            return context?.HttpContext?.Items[CorrelationIdItemKey]?.ToString();
        }

        /// <summary>
        /// Gets the correlation ID from current HttpContext context (extension method)
        /// Implementation for MVC LAYER
        /// </summary>
        public static string GetCorrelationId(this HttpContext context)
        {
            return context?.Items[CorrelationIdItemKey]?.ToString();
        }
    }
}