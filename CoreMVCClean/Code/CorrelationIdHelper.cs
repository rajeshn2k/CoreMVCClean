using Microsoft.AspNetCore.Http;

namespace Core.MVC.Clean
{
    /// <summary>
    /// Helper for accessing correlation ID from HttpContext
    /// </summary>
    public static class CorrelationIdHelper
    {
        private const string CorrelationIdItemKey = "CorrelationId";

        /// <summary>
        /// Gets the correlation ID from current HttpContext
        /// </summary>
        //public static string GetCorrelationId(HttpContext context)
        //{
        //    return context.Items[CorrelationIdItemKey]?.ToString();
        //}

        /// <summary>
        /// Gets the correlation ID from current HttpContext (extension method)
        /// </summary>
        public static string GetCorrelationId(this HttpContext context)
        {
            return context.Items[CorrelationIdItemKey]?.ToString();
        }
    }
}