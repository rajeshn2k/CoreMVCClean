using Serilog.Context;

namespace Core.MVC.Clean
{
    /// <summary>
    /// Middleware to handle correlation IDs for request tracing
    /// </summary>
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private const string CorrelationIdHeader = "X-Correlation-ID";
        private const string CorrelationIdItemKey = "CorrelationId";

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault();

            if (string.IsNullOrEmpty(correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
            }

            //This puts correlation id in context early in the pipeline, so every downstream (ex:API) can access the ID.
            //application should read this correlation id and pass it to downstream (ex:API)
            context.Items[CorrelationIdItemKey] = correlationId;

            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                context.Response.OnStarting(() =>
                {
                    //When a response is rendered X-Correlation-ID also rendered as reference
                    context.Response.Headers[CorrelationIdHeader] = correlationId;
                    return Task.CompletedTask;
                });

                await _next(context);
            }
        }
    }
}