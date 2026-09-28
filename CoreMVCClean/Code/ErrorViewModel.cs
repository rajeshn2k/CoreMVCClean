namespace Core.MVC.Clean
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        // New properties for exception details
        public string? ExceptionMessage { get; set; }
        public string? ExceptionStackTrace { get; set; }
        
        // New properties for API standardization
        public string? Message { get; set; }
        public string? ErrorCode { get; set; }
        public int? StatusCode { get; set; }
    }
}