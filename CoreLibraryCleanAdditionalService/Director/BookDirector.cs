using Newtonsoft.Json;
using System.Text;

namespace Core.Library.Clean.AdditionalService
{
    public class BookDirector : IEntityDirector<BookDTO, BookCreateDTO>
    {
        private readonly HttpClient httpClient;

        public BookDirector(HttpClient _httpClient)
        {
            httpClient = _httpClient;
            Console.WriteLine(httpClient.BaseAddress);
        }

        public async Task<IEnumerable<BookDTO>> GetEntitiesAsync(CancellationToken cancellationToken)
        {
            var requestUrl = "";

            using var response = await httpClient.GetAsync(requestUrl, cancellationToken)
                .ConfigureAwait(false);

            return await HandleResponseAsync<IEnumerable<BookDTO>>(response, cancellationToken);
        }

        public async Task<BookDTO> GetEntityByIdAsync(string entityId, CancellationToken cancellationToken)
        {
            var requestUrl = $"{entityId}";

            using var response = await httpClient.GetAsync(requestUrl, cancellationToken)
                .ConfigureAwait(false);

            return await HandleResponseAsync<BookDTO>(response, cancellationToken);
        }

        public async Task<IEnumerable<BookDTO>> SearchEntitiesAsync(string searchValue, CancellationToken cancellationToken)
        {
            var requestUrl = $"SearchByBook/{searchValue}";

            using var response = await httpClient.GetAsync(requestUrl, cancellationToken)
                .ConfigureAwait(false);

            return await HandleResponseAsync<IEnumerable<BookDTO>>(response, cancellationToken);
        }
        public async Task<IEnumerable<BookDTO>> SearchEntitiesByForeignIdAsync(string foreignKeyId, CancellationToken cancellationToken)
        {
            var requestUrl = $"SearchByPersonId/{foreignKeyId}";

            using var response = await httpClient.GetAsync(requestUrl, cancellationToken)
                .ConfigureAwait(false);

            return await HandleResponseAsync<IEnumerable<BookDTO>>(response, cancellationToken);
        }

        public async Task<long> UpdateEntityByIdAsync(string entityId, BookDTO entity, CancellationToken cancellationToken)
        {
            var requestUrl = $"{entityId}";

            using var content = CreateJsonContent(entity);

            using var response = await httpClient.PutAsync(requestUrl, content, cancellationToken)
                .ConfigureAwait(false);

            return await HandleResponseAsync<long>(response, cancellationToken);
        }

        public async Task<long> UpdateEntitiesAsync(IEnumerable<string> entityIds, IEnumerable<BookDTO> entities, CancellationToken cancellationToken)
        {
            var requestUrl = $"Many/{entityIds}";

            using var content = CreateJsonContent(entities);

            using var response = await httpClient.PutAsync(requestUrl, content, cancellationToken)
                .ConfigureAwait(false);

            return await HandleResponseAsync<long>(response, cancellationToken);
        }

        public async Task<BookDTO> CreateEntityAsync(BookCreateDTO entity, CancellationToken cancellationToken)
        {
            var requestUrl = "";

            using var content = CreateJsonContent(entity);

            using var response = await httpClient.PostAsync(requestUrl, content, cancellationToken)
                .ConfigureAwait(false);

            return await HandleResponseAsync<BookDTO>(response, cancellationToken);
        }

        public async Task<IEnumerable<BookDTO>> CreateEntitiesAsync(IEnumerable<BookCreateDTO> entities, CancellationToken cancellationToken)
        {
            var requestUrl = "Many";

            using var content = CreateJsonContent(entities);

            using var response = await httpClient.PostAsync(requestUrl, content, cancellationToken)
                .ConfigureAwait(false);

            return await HandleResponseAsync<IEnumerable<BookDTO>>(response, cancellationToken);
        }

        public async Task<long> DeleteEntityByIdAsync(string entityId, CancellationToken cancellationToken)
        {
            var requestUrl = $"{entityId}";

            using var response = await httpClient.DeleteAsync(requestUrl, cancellationToken)
                .ConfigureAwait(false);

            return await HandleResponseAsync<long>(response, cancellationToken);
        }

        public async Task<long> DeleteEntitiesAsync(CancellationToken cancellationToken)
        {
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
                var error = await response.Content
                    .ReadAsStringAsync(cancellationToken)
                    .ConfigureAwait(false);

                throw new HttpRequestException(
                    $"Request failed with status code {(int)response.StatusCode} " +
                    $"({response.ReasonPhrase}). Response: {error}");
            }

            var result = await response.Content
                .ReadAsStringAsync(cancellationToken)
                .ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(result))
            {
                return default;
            }

            return JsonConvert.DeserializeObject<T>(result);
        }
    }
}
