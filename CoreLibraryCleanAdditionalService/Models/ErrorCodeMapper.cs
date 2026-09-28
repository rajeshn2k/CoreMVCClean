namespace Core.Library.Clean.AdditionalService
{
    /// <summary>
    /// Maps error codes to user-friendly messages and status codes
    /// </summary>
    public static class ErrorCodeMapper
    {
        public static (string Message, int StatusCode) GetErrorDetails(string errorCode)
        {
            return errorCode switch
            {
                ErrorCodes.NOT_FOUND => ("Resource not found", 404),
                ErrorCodes.BOOK_NOT_FOUND => ("Book not found", 404),
                ErrorCodes.PERSON_NOT_FOUND => ("Person not found", 404),
                ErrorCodes.VALIDATION_ERROR => ("Validation failed", 400),
                ErrorCodes.UNAUTHORIZED => ("Unauthorized access", 401),
                ErrorCodes.FORBIDDEN => ("Access forbidden", 403),
                ErrorCodes.BAD_REQUEST => ("Bad request", 400),
                ErrorCodes.CONFLICT => ("Resource conflict", 409),
                ErrorCodes.BOOK_CREATION_FAILED => ("Failed to create book", 500),
                ErrorCodes.PERSON_CREATION_FAILED => ("Failed to create person", 500),
                ErrorCodes.BOOK_UPDATE_FAILED => ("Failed to update book", 500),
                ErrorCodes.PERSON_UPDATE_FAILED => ("Failed to update person", 500),
                ErrorCodes.BOOK_DELETE_FAILED => ("Failed to delete book", 500),
                ErrorCodes.PERSON_DELETE_FAILED => ("Failed to delete person", 500),
                ErrorCodes.EXTERNAL_API_ERROR => ("External API error", 502),
                ErrorCodes.EXTERNAL_API_TIMEOUT => ("External API timeout", 504),
                ErrorCodes.EXTERNAL_API_UNAVAILABLE => ("External API unavailable", 503),
                _ => ("An error occurred", 500)
            };
        }
    }
}