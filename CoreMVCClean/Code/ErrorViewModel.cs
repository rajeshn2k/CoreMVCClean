namespace Core.MVC.Clean
{
    public class ErrorViewModel
    {
        public string? RequestId { get; init; }

        public string? Path { get; init; }

        public Exception? Exception { get; init; }

        public string ExceptionType => Exception?.GetType().FullName ?? "Unknown";

        public string Message => Exception?.Message ?? "No exception information available.";

        public string Details => Exception?.ToString() ?? "No exception details available.";
    }
}