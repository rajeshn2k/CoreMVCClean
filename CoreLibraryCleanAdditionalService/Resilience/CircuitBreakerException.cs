namespace Core.Library.Clean.AdditionalService.Resilience
{
    /// <summary>
    /// Exception thrown when circuit breaker is open
    /// </summary>
    public class CircuitBreakerOpenException : Exception
    {
        public string ServiceName { get; }
        public DateTime BreakTime { get; }

        public CircuitBreakerOpenException(string serviceName, DateTime breakTime)
            : base($"Circuit breaker is open for service: {serviceName}")
        {
            ServiceName = serviceName;
            BreakTime = breakTime;
        }
    }
}