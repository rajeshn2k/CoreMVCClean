namespace Core.Library.Clean.AdditionalService
{
    /// <summary>
    /// Standardized error codes for API responses
    /// </summary>
    public static class ErrorCodes
    {
        // Common errors
        public const string NOT_FOUND = "NOT_FOUND";
        public const string VALIDATION_ERROR = "VALIDATION_ERROR";
        public const string UNAUTHORIZED = "UNAUTHORIZED";
        public const string FORBIDDEN = "FORBIDDEN";
        public const string INTERNAL_SERVER_ERROR = "INTERNAL_SERVER_ERROR";
        public const string BAD_REQUEST = "BAD_REQUEST";
        public const string CONFLICT = "CONFLICT";

        // Entity-specific errors
        public const string BOOK_NOT_FOUND = "BOOK_NOT_FOUND";
        public const string PERSON_NOT_FOUND = "PERSON_NOT_FOUND";
        public const string BOOK_CREATION_FAILED = "BOOK_CREATION_FAILED";
        public const string PERSON_CREATION_FAILED = "PERSON_CREATION_FAILED";
        public const string BOOK_UPDATE_FAILED = "BOOK_UPDATE_FAILED";
        public const string PERSON_UPDATE_FAILED = "PERSON_UPDATE_FAILED";
        public const string BOOK_DELETE_FAILED = "BOOK_DELETE_FAILED";
        public const string PERSON_DELETE_FAILED = "PERSON_DELETE_FAILED";

        // External API errors
        public const string EXTERNAL_API_ERROR = "EXTERNAL_API_ERROR";
        public const string EXTERNAL_API_TIMEOUT = "EXTERNAL_API_TIMEOUT";
        public const string EXTERNAL_API_UNAVAILABLE = "EXTERNAL_API_UNAVAILABLE";
    }
}