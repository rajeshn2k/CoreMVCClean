using Newtonsoft.Json;
using System.Text;

namespace Core.Library.Clean.AdditionalService
{
    public class PersonDirector : IEntityDirector<PersonDTO, PersonCreateDTO>
    {
        private readonly HttpClient httpClient;

        public PersonDirector(HttpClient _httpClient)
        {
            httpClient = _httpClient;
        }

        public async Task<IEnumerable<PersonDTO>> GetEntitiesAsync(CancellationToken cancellationToken)
        {
            var requestUrl = "";

            using var response = await httpClient.GetAsync(requestUrl, cancellationToken)
                .ConfigureAwait(false);

            return await HandleResponseAsync<IEnumerable<PersonDTO>>(response, cancellationToken);
        }

        public async Task<PersonDTO> GetEntityByIdAsync(string entityId, CancellationToken cancellationToken)
        {
            var requestUrl = $"{entityId}";

            using var response = await httpClient.GetAsync(requestUrl, cancellationToken)
                .ConfigureAwait(false);

            return await HandleResponseAsync<PersonDTO>(response, cancellationToken);
        }

        public async Task<IEnumerable<PersonDTO>> SearchEntitiesAsync(string searchValue, CancellationToken cancellationToken)
        {
            var requestUrl = $"SearchByPerson/{searchValue}";

            using var response = await httpClient.GetAsync(requestUrl, cancellationToken)
                .ConfigureAwait(false);

            return await HandleResponseAsync<IEnumerable<PersonDTO>>(response, cancellationToken);
        }

        public async Task<IEnumerable<PersonDTO>> SearchEntitiesByForeignIdAsync(string foreignKeyId, CancellationToken cancellationToken)
        {
            var requestUrl = $"SearchByBookId/{foreignKeyId}";

            using var response = await httpClient.GetAsync(requestUrl, cancellationToken)
                .ConfigureAwait(false);

            return await HandleResponseAsync<IEnumerable<PersonDTO>>(response, cancellationToken);
        }

        public async Task<long> UpdateEntityByIdAsync(string entityId, PersonDTO entity, CancellationToken cancellationToken)
        {
            var requestUrl = $"{entityId}";

            using var content = CreateJsonContent(entity);

            using var response = await httpClient.PutAsync(requestUrl, content, cancellationToken)
                .ConfigureAwait(false);

            return await HandleResponseAsync<long>(response, cancellationToken);
        }

        public async Task<long> UpdateEntitiesAsync(IEnumerable<string> entityIds, IEnumerable<PersonDTO> entities, CancellationToken cancellationToken)
        {
            var requestUrl = $"Many/{entityIds}";

            using var content = CreateJsonContent(entities);

            using var response = await httpClient.PutAsync(requestUrl, content, cancellationToken)
                .ConfigureAwait(false);

            return await HandleResponseAsync<long>(response, cancellationToken);
        }

        public async Task<PersonDTO> CreateEntityAsync(PersonCreateDTO entity, CancellationToken cancellationToken)
        {
            var requestUrl = "";

            using var content = CreateJsonContent(entity);

            using var response = await httpClient.PostAsync(requestUrl, content, cancellationToken)
                .ConfigureAwait(false);

            return await HandleResponseAsync<PersonDTO>(response, cancellationToken);
        }

        public async Task<IEnumerable<PersonDTO>> CreateEntitiesAsync(IEnumerable<PersonCreateDTO> entities, CancellationToken cancellationToken)
        {
            var requestUrl = "Many";

            using var content = CreateJsonContent(entities);

            using var response = await httpClient.PostAsync(requestUrl, content, cancellationToken)
                .ConfigureAwait(false);

            return await HandleResponseAsync<IEnumerable<PersonDTO>>(response, cancellationToken);
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
