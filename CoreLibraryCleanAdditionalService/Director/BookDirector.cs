using Newtonsoft.Json;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Core.Library.Clean.AdditionalService
{
    public class BookDirector : IEntityDirector<BookDTO, BookCreateDTO>
    {
        private readonly HttpClient httpClient;
        private readonly IHttpContextAccessor httpContextAccessor;

        public BookDirector(HttpClient _httpClient, IHttpContextAccessor _httpContextAccessor)
        {
            httpClient = _httpClient;
            httpContextAccessor = _httpContextAccessor;
            Console.WriteLine(httpClient.BaseAddress);
        }

        private void AddCorrelationIdHeader()
        {
            var correlationId = httpContextAccessor?.HttpContext?.Items["CorrelationId"]?.ToString();
            if (!string.IsNullOrEmpty(correlationId))
            {
                httpClient.DefaultRequestHeaders.Remove("X-Correlation-ID");
                httpClient.DefaultRequestHeaders.Add("X-Correlation-ID", correlationId);
            }
        }

        public async Task<IEnumerable<BookDTO>> GetEntitiesAsync(CancellationToken cancellationToken)
        {
            AddCorrelationIdHeader();
            var requestUrl = "";

            using var response = await httpClient.GetAsync(requestUrl, cancellationToken)
                .ConfigureAwait(false);

            return await HandleResponseAsync<IEnumerable<BookDTO>>(response, cancellationToken);
        }

        public async Task<BookDTO> GetEntityByIdAsync(string entityId, CancellationToken cancellationToken)
        {
            AddCorrelationIdHeader();
            var requestUrl = $"{entityId}";

            using var response = await httpClient.GetAsync(requestUrl, cancellationToken)
                .ConfigureAwait(false);

            return await HandleResponseAsync<BookDTO>(response, cancellationToken);
        }

        public async Task<IEnumerable<BookDTO>> SearchEntitiesAsync(string searchValue, CancellationToken cancellationToken)
        {
            AddCorrelationIdHeader();
            var requestUrl = $"SearchByBook/{searchValue}";

            using var response = await httpClient.GetAsync(requestUrl, cancellationToken)
                .ConfigureAwait(false);

            return await HandleResponseAsync<IEnumerable<BookDTO>>(response, cancellationToken);
        }
        public async Task<IEnumerable<BookDTO>> SearchEntitiesByForeignIdAsync(string foreignKeyId, CancellationToken cancellationToken)
        {
            AddCorrelationIdHeader();
            var requestUrl = $"SearchByPersonId/{foreignKeyId}";

            using var response = await httpClient.GetAsync(requestUrl, cancellationToken)
                .ConfigureAwait(false);

            return await HandleResponseAsync<IEnumerable<BookDTO>>(response, cancellationToken);
        }

        public async Task<long> UpdateEntityByIdAsync(string entityId, BookDTO entity, CancellationToken cancellationToken)
        {
            AddCorrelationIdHeader();
            var requestUrl = $"{entityId}";

            using var content = CreateJsonContent(entity);

            using var response = await httpClient.PutAsync(requestUrl, content, cancellationToken)
                .ConfigureAwait(false);

            return await HandleResponseAsync<long>(response, cancellationToken);
        }

        public async Task<long> UpdateEntitiesAsync(IEnumerable<string> entityIds, IEnumerable<BookDTO> entities, CancellationToken cancellationToken)
        {
            AddCorrelationIdHeader();
            var requestUrl = $"Many/{entityIds}";

            using var content = CreateJsonContent(entities);

            using var response = await httpClient.PutAsync(requestUrl, content, cancellationToken)
                .ConfigureAwait(false);

            return await HandleResponseAsync<long>(response, cancellationToken);
        }

        public async Task<BookDTO> CreateEntityAsync(BookCreateDTO entity, CancellationToken cancellationToken)
        {
            AddCorrelationIdHeader();
            var requestUrl = "";

            using var content = CreateJsonContent(entity);

            using var response = await httpClient.PostAsync(requestUrl, content, cancellationToken)
                .ConfigureAwait(false);

            return await HandleResponseAsync<BookDTO>(response, cancellationToken);
        }

        public async Task<IEnumerable<BookDTO>> CreateEntitiesAsync(IEnumerable<BookCreateDTO> entities, CancellationToken cancellationToken)
        {
            AddCorrelationIdHeader();
            var requestUrl = "Many";

            using var content = CreateJsonContent(entities);

            using var response = await httpClient.PostAsync(requestUrl, content, cancellationToken)
                .ConfigureAwait(false);

            return await HandleResponseAsync<IEnumerable<BookDTO>>(response, cancellationToken);
        }

        public async Task<long> DeleteEntityByIdAsync(string entityId, CancellationToken cancellationToken)
        {
            AddCorrelationIdHeader();
            var requestUrl = $"{entityId}";

            using var response = await httpClient.DeleteAsync(requestUrl, cancellationToken)
                .ConfigureAwait(false);

            return await HandleResponseAsync<long>(response, cancellationToken);
        }

        public async Task<long> DeleteEntitiesAsync(CancellationToken cancellationToken)
        {
            AddCorrelationIdHeader();
            var requestUrl = $"Many";

            using var response = await httpClient.DeleteAsync(requestUrl, cancellationToken)
                .ConfigureAwait(false);

            return await HandleResponseAsync<long>(response, cancellationToken);
        }

        private static StringContent CreateJsonContent(object content)
        {
            var json = JsonConvert.SerializeObject(content);

            return new StringContent(
                json,
                Encoding.UTF8,
                "application/json");
        }

        private static async Task<T> HandleResponseAsync<T>(
            HttpResponseMessage response,
            CancellationToken cancellationToken)
        {
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content
                    .ReadAsStringAsync(cancellationToken)
                    .ConfigureAwait(false);

                try
                {
                    var errorResponse = JsonConvert.DeserializeObject<ApiErrorResponse>(errorContent);
                    if (errorResponse != null)
                    {
                        throw new ApiException(
                            errorResponse.Error.Code,
                            errorResponse.Error.Message,
                            errorResponse.Error.StatusCode);
                    }
                }
                catch
                {
                }

                var errorCode = response.StatusCode switch
                {
                    System.Net.HttpStatusCode.NotFound => ErrorCodes.NOT_FOUND,
                    System.Net.HttpStatusCode.BadRequest => ErrorCodes.BAD_REQUEST,
                    System.Net.HttpStatusCode.Unauthorized => ErrorCodes.UNAUTHORIZED,
                    System.Net.HttpStatusCode.Forbidden => ErrorCodes.FORBIDDEN,
                    _ => ErrorCodes.INTERNAL_SERVER_ERROR
                };

                var (message, statusCode) = ErrorCodeMapper.GetErrorDetails(errorCode);

                throw new ApiException(errorCode, message, statusCode);
            }

            var result = await response.Content
                .ReadAsStringAsync(cancellationToken)
                .ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(result))
            {
                return default;
            }

            try
            {
                var wrappedResponse = JsonConvert.DeserializeObject<ApiResponse<T>>(result);
                if (wrappedResponse != null && wrappedResponse.Success)
                {
                    return wrappedResponse.Data;
                }
            }
            catch
            {
            }

            return JsonConvert.DeserializeObject<T>(result);
        }
    }
}
